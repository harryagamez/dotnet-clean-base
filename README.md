# dotnet-clean-base

Línea base para proyectos backend en **.NET 10** con **Clean Architecture**. Trae un núcleo compartido (Domain, Application, Infrastructure) y dos hosts que lo reutilizan: una **Web API** con Minimal APIs y una **Azure Function App** con HTTP triggers. Incluye un ejemplo completo (CRUD de productos), tests en todas las capas y CI.

Sirve como plantilla: generas una solución nueva con tu propio nombre y empiezas a trabajar sobre una base que ya compila, pasa los tests y sigue buenas prácticas.

## Qué incluye

- **Clean Architecture** con la regla de dependencias verificada por tests de arquitectura.
- **CQRS sin MediatR**: comandos y consultas con handlers propios; validación y logging como **decoradores**.
- **Result pattern**: los errores de negocio son valores, no excepciones.
- **Validación en dos niveles**: FluentValidation para la entrada y una entidad de dominio que protege sus invariantes.
- **EF Core + SQL Server** con configuraciones por entidad, repositorio y unidad de trabajo.
- **Errores HTTP estándar** con ProblemDetails (RFC 9457) en ambos hosts.
- **OpenAPI + Scalar**, health checks y manejo global de excepciones.
- **Tests**: unitarios (Domain, Application, Functions), de arquitectura y de integración contra un SQL Server real (Testcontainers).
- **Calidad**: Central Package Management, analizadores con *warnings as errors*, `.editorconfig` y `dotnet format` en el CI.
- **Plantilla `dotnet new`** para generar soluciones con otro nombre.

## Arquitectura

```
          ┌───────────────┐   ┌──────────────────────┐
 Entrada  │      Api      │   │     FunctionApp      │   Hosts: traducen HTTP ⇄ casos de uso
          └───────┬───────┘   └──────────┬───────────┘   y son la raíz de composición (DI)
                  └───────────┬──────────┘
                     ┌────────▼─────────┐
                     │  Infrastructure  │   EF Core, repositorios, servicios externos
                     └────────┬─────────┘
                     ┌────────▼─────────┐
                     │   Application    │   Casos de uso (comandos/consultas), validación
                     └────────┬─────────┘
                     ┌────────▼─────────┐
                     │      Domain      │   Entidades, reglas de negocio, contratos
                     └──────────────────┘
```

**Regla de dependencias:** cada capa solo conoce a las de abajo. El Domain no depende de nada. Infrastructure *implementa* los contratos que define el Domain (`IProductRepository`) o Application (`IUnitOfWork`); eso es inversión de dependencias. Los hosts son el único lugar que conoce todas las capas, porque ahí se registran las dependencias.

Los dos hosts llaman a **los mismos casos de uso** con las mismas líneas de registro:

```csharp
builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration);
```

Cambiar o añadir un host (un Worker Service, Functions con Service Bus...) no toca el núcleo.

## Estructura

```
src/
  CleanBase.Domain/            Entidades, Result/Error, contratos de repositorio
  CleanBase.Application/       Comandos, consultas, handlers, validadores, decoradores
  CleanBase.Infrastructure/    DbContext, configuraciones EF, repositorios, migraciones
  Hosts/
    CleanBase.Api/             Minimal APIs, ProblemDetails, OpenAPI
    CleanBase.FunctionApp/     Azure Functions (isolated worker), carpeta Functions/
tests/
  CleanBase.Domain.UnitTests/
  CleanBase.Application.UnitTests/
  CleanBase.ArchitectureTests/
  CleanBase.Api.IntegrationTests/
  CleanBase.FunctionApp.UnitTests/
```

Dentro de cada capa el código se organiza **por feature** (`Products/Create`, `Products/GetById`...) y no por tipo técnico. Así todo lo de un caso de uso queda junto.

## Requisitos

- [.NET SDK 10](https://dotnet.microsoft.com/download) (la versión mínima está fijada en `global.json`).
- SQL Server: LocalDB en Windows (viene con Visual Studio) o cualquier otra instancia.
- Para los tests de integración: **Docker**, o una base de datos propia (ver [Tests](#tests)).
- Para ejecutar las Functions en local: [Azure Functions Core Tools](https://learn.microsoft.com/azure/azure-functions/functions-run-local) v4.

## Crear un proyecto nuevo a partir de esta base

**Opción A: plantilla `dotnet new` (recomendada).** Instálala una vez desde una copia local del repositorio:

```powershell
git clone <url-de-este-repo> dotnet-clean-base
dotnet new install ./dotnet-clean-base
```

Y luego, para cada proyecto nuevo:

```powershell
dotnet new cleanbase -n Acme.Billing -o D:\repos\acme-billing
```

Todo `CleanBase` se reemplaza por `Acme.Billing`: proyectos, carpetas, namespaces, la solución y el nombre de la base de datos. El resultado no trae el historial de git de este repositorio. Para actualizar la plantilla después de un `git pull`, vuelve a ejecutar `dotnet new install` con `--force`.

**Opción B: botón *Use this template* de GitHub.** Crea un repositorio nuevo con una copia limpia. Tendrás que renombrar `CleanBase` a mano.

## Ejecutar

```powershell
dotnet tool restore   # instala dotnet-ef (herramienta local del repo)
dotnet build
```

### Web API

```powershell
dotnet run --project src/Hosts/CleanBase.Api
```

En el entorno Development la API **aplica las migraciones al arrancar** y publica la documentación en `/scalar` (UI) y `/openapi/v1.json`. El archivo `CleanBase.Api.http` tiene peticiones de ejemplo para Visual Studio, Rider o VS Code.

La cadena de conexión está en `appsettings.Development.json` y apunta a LocalDB. Para no versionar credenciales, usa *user secrets*:

```powershell
dotnet user-secrets set "ConnectionStrings:Database" "<cadena>" --project src/Hosts/CleanBase.Api
```

### Azure Functions

```powershell
cd src/Hosts/CleanBase.FunctionApp
copy local.settings.example.json local.settings.json   # local.settings.json está en .gitignore
func start
```

Las funciones quedan en `http://localhost:7071/api/products`. Usan `AuthorizationLevel.Function`: en local la clave no se exige, pero en Azure hay que enviarla (`?code=` o la cabecera `x-functions-key`). La base de datos debe existir; créala ejecutando la API una vez o con `dotnet ef database update` (ver abajo).

### Migraciones

```powershell
# Crear una migración
dotnet ef migrations add <Nombre> --project src/CleanBase.Infrastructure --startup-project src/Hosts/CleanBase.Api --output-dir Database/Migrations

# Aplicarla
dotnet ef database update --project src/CleanBase.Infrastructure --startup-project src/Hosts/CleanBase.Api
```

En producción no conviene migrar al arrancar la aplicación. Lo habitual es generar un script idempotente (`dotnet ef migrations script --idempotent`) o un *migration bundle* y aplicarlo como paso del despliegue.

## Tests

```powershell
dotnet test
```

Los tests usan **xUnit v3** sobre **Microsoft Testing Platform** (activada en `global.json`), con **Shouldly** para las aserciones y **NSubstitute** para los dobles.

| Proyecto | Qué prueba |
|----------|------------|
| `Domain.UnitTests` | Invariantes de la entidad y el tipo `Result`. |
| `Application.UnitTests` | Handlers con repositorios simulados, el decorador de validación y que la cadena de decoradores se registre bien. |
| `ArchitectureTests` | La regla de dependencias entre capas y las convenciones (handlers `internal sealed`). |
| `Api.IntegrationTests` | La API completa en memoria (`WebApplicationFactory`) contra un SQL Server real: HTTP, validación, EF Core y migraciones. |
| `FunctionApp.UnitTests` | Las funciones como clases normales, sin el runtime de Functions. |

Los tests de integración levantan un **SQL Server desechable en Docker** con Testcontainers. Si no tienes Docker, apunta la variable `INTEGRATION_TESTS_DATABASE` a una base de datos **dedicada a tests**:

```powershell
$env:INTEGRATION_TESTS_DATABASE = "Server=(localdb)\MSSQLLocalDB;Database=CleanBase_Tests;Trusted_Connection=True;TrustServerCertificate=True"
dotnet test
```

## Decisiones de diseño

### Result pattern en vez de excepciones

Un producto que no existe o un SKU repetido no son situaciones excepcionales: son resultados esperados de la operación. Los handlers devuelven `Result` o `Result<T>` con un `Error` que tiene código, descripción y tipo (`Validation`, `NotFound`, `Conflict`...). Cada host traduce el tipo de error a su protocolo; en HTTP, a 400, 404 o 409 con ProblemDetails. Las excepciones quedan para lo inesperado y las atrapa el manejador global, que devuelve un 500 genérico sin filtrar detalles internos.

### CQRS sin MediatR: handlers inyectados y decoradores

MediatR pasó a tener licencia comercial. En lugar de cambiarlo por otro mediador, esta base usa el enfoque más simple: **los endpoints reciben directamente el handler que necesitan** (`ICommandHandler<CreateProductCommand, Guid>`). La firma del endpoint dice exactamente qué caso de uso ejecuta (con "Ir a la implementación" llegas al handler) y no hay un despachador que busque handlers por reflexión en cada petición.

Los aspectos transversales se aplican con el **patrón Decorator** usando [Scrutor](https://github.com/khellang/Scrutor):

```
Endpoint → LoggingDecorator → ValidationDecorator → Handler
```

Cada decorador implementa la misma interfaz que el handler y lo envuelve. Para añadir otro aspecto (métricas, transacciones, caché) se escribe un decorador y se registra con una línea en `AddApplication()`.

### Validación en dos niveles

- **FluentValidation** (Application) valida la forma de la entrada y devuelve **todos** los errores a la vez, uno por campo, que es lo que espera un cliente.
- **La entidad** (Domain) protege sus invariantes. `Product.Create` y `Product.Update` nunca dejan un producto en estado inválido, aunque alguien la use sin pasar por el validador. Es el modelo de *entidad siempre válida*.

Parece duplicación, pero cada nivel tiene un propósito distinto: la experiencia del cliente y la integridad del modelo.

### Repositorio + unidad de trabajo

Los repositorios solo **preparan** cambios (`Add`, `Remove`) y el handler decide cuándo confirmarlos con `IUnitOfWork.SaveChangesAsync()`. Así una operación que toca varios agregados se guarda en una sola transacción. `ApplicationDbContext` implementa `IUnitOfWork` directamente: el `DbContext` ya es una unidad de trabajo, y envolverlo en otra clase no aportaría nada.

### Otros detalles

- **GUID v7** (`Guid.CreateVersion7()`) como identificador: se generan en la aplicación sin consultar la base y, como están ordenados por tiempo, no fragmentan el índice clustered como los GUID aleatorios.
- **Contratos HTTP separados de los comandos** (`CreateProductRequest` y `CreateProductCommand`): la API puede evolucionar (versiones, campos renombrados) sin tocar Application.
- **Tipos `internal` por defecto**: handlers, validadores, repositorios y configuraciones no son públicos. Cada capa solo expone lo que las demás necesitan; los tests acceden con `InternalsVisibleTo`.
- **`[LoggerMessage]`**: el código de logging se genera al compilar, sin reservas de memoria por llamada.
- **Central Package Management** (`Directory.Packages.props`): todas las versiones de paquetes en un solo archivo.

## Añadir un feature nuevo

1. **Domain:** la entidad con sus invariantes, sus errores (`XxxErrors`) y el contrato del repositorio.
2. **Application:** una carpeta por caso de uso con el comando o consulta, su handler (`internal sealed`) y su validador. El registro es automático.
3. **Infrastructure:** la configuración EF (`IEntityTypeConfiguration<T>`), el repositorio, su registro en `AddInfrastructure()` y una migración.
4. **Hosts:** los endpoints o funciones, que solo traducen HTTP ⇄ comando o consulta.
5. **Tests** en cada nivel.

## Quitar el ejemplo de productos

1. Borra las carpetas `Products/` de cada proyecto, en `src` y en `tests`.
2. Quita sus referencias: la propiedad `Products` de `ApplicationDbContext`, el registro de `ProductRepository` en `AddInfrastructure()`, la llamada a `MapProductEndpoints()` en la API y la migración `InitialCreate`.
3. En `Application.UnitTests`, `DependencyInjectionTests` y `ValidationDecoratorTests` usan el ejemplo: adáptalos a tu primer feature.

El compilador te señala cualquier referencia que quede. El registro de handlers y decoradores no falla aunque no haya ninguno (usa `TryDecorate`).

## Próximos pasos

- Autenticación y autorización (JWT / Microsoft Entra ID).
- Paginación y filtros en los listados.
- Observabilidad con OpenTelemetry en ambos hosts.
- Eventos de dominio.
- Traducir la violación del índice único (dos peticiones simultáneas con el mismo SKU) a un `409` en vez de un `500`.

## Licencia

[MIT](LICENSE)

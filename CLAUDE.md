# dotnet-clean-base

Línea base .NET 10 con Clean Architecture: núcleo compartido + hosts Web API y Azure Functions. Se distribuye como plantilla `dotnet new` (`cleanbase`). El README explica las decisiones de diseño; este archivo es para trabajar en el repo.

## Comandos

```powershell
dotnet tool restore                          # dotnet-ef local
dotnet build                                 # warnings = errores (analizadores + estilo)
dotnet test                                  # Microsoft Testing Platform (global.json)
dotnet format --verify-no-changes            # el CI lo exige
dotnet run --project src/Hosts/CleanBase.Api # Development: migra al arrancar, UI en /scalar
```

En esta máquina **no hay Docker**: para los tests de integración hay que definir antes
`$env:INTEGRATION_TESTS_DATABASE = "Server=(localdb)\MSSQLLocalDB;Database=CleanBase_Tests;Trusted_Connection=True;TrustServerCertificate=True"`.
Tampoco están instaladas las Azure Functions Core Tools: la FunctionApp compila y tiene tests unitarios, pero no se ha ejecutado con `func start`.

Migraciones: `dotnet ef migrations add <Nombre> --project src/CleanBase.Infrastructure --startup-project src/Hosts/CleanBase.Api --output-dir Database/Migrations`.

## Convenciones

- Código, identificadores y comentarios en **inglés**; README y documentación en **español**.
- Organización por feature: `Products/Create/{Command,Handler,Validator}.cs`.
- Handlers, validadores, repositorios y configuraciones EF son `internal sealed` (los tests de arquitectura lo verifican). Los tests acceden vía `InternalsVisibleTo`.
- Errores de negocio con `Result`/`Error`, nunca con excepciones. Catálogo por entidad en `XxxErrors`.
- Los handlers se inyectan directamente (sin mediador). Los decoradores se registran en `Application/DependencyInjection.cs`: el último `TryDecorate` es el más externo.
- Campos privados `_camelCase`; `const` y `static readonly` en `PascalCase`.
- Tests: `Metodo_Escenario_Resultado`, Shouldly + NSubstitute, y `TestContext.Current.CancellationToken` en toda llamada que acepte un token (el analizador xUnit1051 lo exige).
- Versiones de paquetes solo en `Directory.Packages.props`.

## Plantilla

- `.template.config/template.json`: `sourceName` = `CleanBase`, sin opciones condicionales (decisión deliberada).
- No usar la cadena `CleanBase` para nada que no deba renombrarse. El workflow `template.yml` genera `Acme.Billing`, comprueba que no quede `CleanBase` y ejecuta build y tests.
- Se excluyen de lo generado: `README.md` (se usa `README.template.md`), `CLAUDE.md`, `LICENSE` y `template.yml`.
- Probar en local: `dotnet new install ./`, generar en una carpeta temporal y luego `dotnet new uninstall <ruta>`.

## Git

- Rama `main`. Commits **sin** atribución a Claude (sin `Co-Authored-By`).

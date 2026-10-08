# CleanBase

Solución .NET 10 con Clean Architecture, generada con la plantilla `dotnet-clean-base`.

## Primeros pasos

```powershell
dotnet tool restore
dotnet build
dotnet test
```

Para ejecutar la API (en Development aplica las migraciones al arrancar):

```powershell
dotnet run --project src/Hosts/CleanBase.Api
```

Para ejecutar las Azure Functions, copia `src/Hosts/CleanBase.FunctionApp/local.settings.example.json` a `local.settings.json` y ejecuta `func start` desde esa carpeta (necesitas Azure Functions Core Tools).

## Siguientes pasos

- Ajusta la cadena de conexión en `appsettings.Development.json` y en `local.settings.json`.
- Borra el ejemplo de productos si no lo necesitas: las carpetas `Products/` de cada capa y de los tests, y la migración inicial (después genera una nueva).
- Quita el host que no uses (`src/Hosts/...`) junto con su proyecto de tests y su línea en el `.slnx`.

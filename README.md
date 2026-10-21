# Laboratorio 10 - Hotel.Web

Aplicación ASP.NET Core MVC con Entity Framework Core Database First. No usa migraciones ni procedimientos almacenados.

## Puesta en marcha

1. Ejecutar `HotelDB.sql` en `LaizonSorlo\SQLEXPRESS`.
2. Ejecutar `ALTER TABLE Reserva ADD Observaciones NVARCHAR(200) NULL;` en `HotelDB`.
3. Abrir `Hotel.slnx` con Visual Studio 2026 o abrir la carpeta con Visual Studio Code.
4. Ejecutar `dotnet restore`, `dotnet build` y `dotnet run --project Hotel.Web`.

La cadena de conexión está en `Hotel.Web/appsettings.json` y usa autenticación integrada de Windows.

## Estructura

- `Models`: entidades y contexto generados desde SQL Server, sin edición manual.
- `Repositorios`: consultas LINQ, listas desplegables y operaciones `SaveChangesAsync`.
- `Controllers`: validación web y flujo Post/Redirect/Get; no acceden directamente a `HotelContext`.
- `Views`: vistas fuertemente tipadas para habitaciones, disponibilidad y reservas.

## Repositorio

<https://github.com/LaizonSorlo21/Lab10-Arevalo>

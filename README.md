## SmartPantry

Repositorio del Trabajo Práctico Integrador de Desarrollo de Software 2026.

## Pre-requisitos

- IDE: Visual Studio 2022 o 2026 (con desarrollo de ASP.NET y web)
- Entorno de ejecución: Node.js 24.15.0 o superior
- Gestor de paquetes: Yarn 1.22.x
- Bases de datos: SQL Server Developer o Express y SQL Server Management Studio (SSMS)
- Creador de soluciones: ABP Studio

## Configuración local

- Clonar el repositorio: `git clone https://github.com/DS26-moreno-iribarren-gallegos/SmartPantry.git`.
- Configurar la base de datos: actualizar la cadena de conexión de los archivos appsettings.json de las carpetas DbMigrator y HttpApi.Host para apuntar al servidor local: `"Default": "Server=(LocalDb)\\MSSQLLocalDB;Database=SmartPantry1;Trusted_Connection=True;TrustServerCertificate=true"`.
- Posicionarse en la raiz del directorio de SmartPantry, luego ejecutar el siguiente comando de .NET para evitar el error "UntrustedRoot": `dotnet dev-certs https --trust`.

# Puesta en marcha

## Instalacion de librerias 

- Posicionarse en la raiz del directorio de SmartPantry, luego ejecutar `abp install-libs` para instalar las librerias faltantes.
- Posicionarse en la carpeta `angular` e instalar las dependencias restantes ejecutando `yarn install` y `yarn add --dev @vitest/browser-playwright playwright`

## Compilación del proyecto

- Ejecutar:  `dotnet restore .\SmartPantry1.slnx` y `dotnet build .\SmartPantry1.slnx --configuration Debug --no-restore` para compilar el proyecto.

## Crear tablas SQL

- Ejecutar el siguiente comando para generar las tablas SQL: `dotnet run --project .\src\SmartPantry1.DbMigrator`

# Prueba de interfaz

- Ejecutar los siguientes comandos:
- `dotnet run --project .\src\SmartPantry1.HttpApi.Host`
- `Push-Location .\angular`
- `yarn start`
- Con los comandos corriendo, verificar en `http://localhost:4200/` y `https://localhost:44381/` el correcto despliegue de la interfaz.

# Verificación

## Backend

- Posicionarse en la raiz del proyecto, y ejecutar los siguientes comandos:
- `dotnet restore ./SmartPantry.slnx`
- `dotnet build ./SmartPantry.slnx --configuration Release --no-restore`
- `dotnet test ./SmartPantry.slnx --configuration Release --no-build`

## Frontend

- Posicionarse en la carpeta angular, y ejecutar los siguientes comandos:
- `yarn build`
- `yarn test --watch=false --browsers=ChromeHeadless`

# Integrantes

- Gonzalo Iribarren, Gonzalo1877
- Marco Gallegos, mmarcogallegos
- Franco Moreno, nomore712


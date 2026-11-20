## SmartPantry

Repositorio del Trabajo Práctico Integrador de Desarrollo de Software 2026.

## Pre-requisitos

- IDE: Visual Studio 2022 o 2026 (con desarrollo de ASP.NET y web)
- Entorno de ejecución: Node.js 24.15.0 o superior
- Gestor de paquetes: Yarn 1.22.x
- Bases de datos: SQL Server Developer o Express y SQL Server Management Studio (SSMS)
- Creador de soluciones: ABP Studio

## Configuración local

- Clonar el repositorio:
 `git clone https://github.com/DS26-moreno-iribarren-gallegos/SmartPantry.git`.
- Configurar la base de datos: actualizar la cadena de conexión de los archivos appsettings.json de las carpetas DbMigrator y HttpApi.Host para apuntar al servidor local:
 `"Default": "Server=(LocalDb)\\MSSQLLocalDB;Database=SmartPantry;Trusted_Connection=True;TrustServerCertificate=true"`.
- Posicionarse en la raiz del directorio de SmartPantry, luego ejecutar el siguiente comando de .NET para evitar el error "UntrustedRoot":
 `dotnet dev-certs https --trust`.

## Gestión de Despensa Personal (RF-11)

Funcionalidad full-stack para que cada usuario autenticado gestione su propia despensa doméstica con aislamiento estricto por usuario (`OwnerUserId`).

### Endpoints principales (`/api/app/pantry-item`)
- `GET /api/app/pantry-item`: Lista paginada de los ítems pertenecientes al usuario autenticado, resolviendo el nombre desde el catálogo (`Products`) por `ProductBarcode` cuando existe.
- `GET /api/app/pantry-item/{id}`: Obtiene el detalle de un ítem propio (`403 Forbidden` si pertenece a otro usuario).
- `POST /api/app/pantry-item`: Registra un nuevo ítem en la despensa del usuario (`quantity`, `unit` y `expirationDate` opcionales).
- `PUT /api/app/pantry-item/{id}`: Actualiza cantidad, unidad o fecha de vencimiento de un ítem propio.
- `POST /api/app/pantry-item/{id}/mark-as-consumed`: Marca un ítem disponible como consumido conservándolo como historial.
- `DELETE /api/app/pantry-item/{id}`: Elimina un ítem propio.

### Interfaz Web Angular (`/pantry`)
- Accesible desde el menú lateral **Mi despensa** (*My Pantry*) para cualquier usuario autenticado.
- Soporte completo de localización en **Español (`es`)** e **Inglés (`en`)** y formateo regional de fechas.
- Manejo explícito de estados de **carga**, **error con reintento**, **lista vacía** y **resultados**, además de filtrado por estado (**Todos**, **Disponibles**, **Consumidos**).

### Ejecución de pruebas automatizadas
- **Backend (.NET / xUnit):**
  ```bash
  dotnet test

# Puesta en marcha

## Instalacion de librerias 

- Posicionarse en la raiz del directorio de SmartPantry, luego ejecutar `abp install-libs` para instalar las librerias faltantes.
- Posicionarse en la carpeta `angular` e instalar las dependencias restantes ejecutando `yarn install` y `yarn add --dev @vitest/browser-playwright playwright`

## Compilación del proyecto

- Ejecutar:  `dotnet restore .\SmartPantry.slnx` y `dotnet build .\SmartPantry.slnx --configuration Debug --no-restore` para compilar el proyecto.

## Crear tablas SQL

- Ejecutar el siguiente comando para generar las tablas SQL:
 `dotnet run --project .\src\SmartPantry.DbMigrator`

# Prueba de interfaz

- Ejecutar los siguientes comandos:
- `dotnet run --project .\src\SmartPantry.HttpApi.Host`
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


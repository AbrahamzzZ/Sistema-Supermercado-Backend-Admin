# Sistema Supermercado — Admin

Microservicio de **datos maestros y reportes** del supermercado: productos, categorías, clientes, proveedores,
transportistas, sucursales, ofertas y datos del negocio.

> 📘 Para levantar **todo el sistema** (Docker, Kafka, Gateway, etc.) ver el
> [README principal en el repo del Gateway](https://github.com/AbrahamzzZ/Sistema-Supermercado-Backend-Gateway#readme).

## Responsabilidad

- CRUD de los datos maestros que usan el resto de servicios
- Reportes del negocio (productos más vendidos/comprados, top clientes y proveedores, etc.)
- Análisis con IA de los productos más vendidos usando **Ollama** (`qwen3:8b`)
- Ubicación geográfica de sucursales (tipo `geography` de SQL Server con NetTopologySuite)

**Base de datos:** `DB_Admin` — tablas `PRODUCTO`, `CATEGORIA`, `CLIENTE`, `PROVEEDOR`, `TRANSPORTISTA`,
`SUCURSAL`, `OFERTA`, `NEGOCIO`.

## Estructura

```
Admin/           API (controllers, Program.cs, appsettings)
Domain/          Entidades, DTOs, DbContext, configuraciones EF, Mapper y Migrations
Infrastructure/  Repositorios, servicios y validadores (FluentValidation)
Utilities/       ApiResponse, mensajes, paginación y cliente de Ollama
```

## Endpoints

Vía Gateway: `http://localhost:8000/admin/...` → aquí `/api/...` · **Todos requieren token.**

Cada uno de estos recursos tiene el mismo CRUD:
`Producto`, `Categoria`, `Cliente`, `Proveedor`, `Transportista`, `Sucursal`, `Oferta`

| Método | Ruta | Descripción |
|---|---|---|
| GET | `/api/{Recurso}` | Lista todos |
| GET | `/api/{Recurso}/paginacion?pageNumber=1&pageSize=10` | Lista paginada |
| GET | `/api/{Recurso}/{id}` | Obtiene uno (404 si no existe) |
| POST | `/api/{Recurso}` | Registra |
| PUT | `/api/{Recurso}/{id}` | Edita |
| DELETE | `/api/{Recurso}/{id}` | Elimina |

Negocio y reportes:

| Método | Ruta |
|---|---|
| GET / PUT | `/api/Negocio/{id}` |
| GET | `/api/Negocio/producto-mas-comprado` |
| GET | `/api/Negocio/producto-mas-vendido` |
| GET | `/api/Negocio/producto-mas-vendido-ia` *(usa Ollama)* |
| GET | `/api/Negocio/top-clientes` |
| GET | `/api/Negocio/top-proveedores` |
| GET | `/api/Negocio/viajes-transportista` |
| GET | `/api/Negocio/empleados-productivos` |

Swagger: http://localhost:8001/swagger (Docker) · https://localhost:7047/swagger (local)

## Quién consume este servicio

**Order** e **Inventory** llaman a `GET /admin/{Recurso}/{id}` **a través del Gateway** para validar que un
producto, cliente, proveedor, sucursal o transportista existe y para mostrar sus datos.

## Configuración

| Clave | Descripción |
|---|---|
| `ConnectionStrings:CadenaSQL` | Cadena de conexión a `DB_Admin` |
| `Jwt:Authority` | URL de Auth, de donde se descarga la clave pública (local: `https://localhost:7186`) |
| `Jwt:RequireHttpsMetadata` | `false` solo dentro de Docker (http) |
| `Jwt:Issuer` / `Jwt:Audience` | Deben coincidir con los de Auth |
| `Ollama:BaseUrl` | URL de Ollama (local: `http://localhost:11434`; Docker: `http://host.docker.internal:11434`) |

Este servicio **no tiene ninguna clave JWT**: solo valida tokens con la clave pública que publica Auth.

## Ejecutar solo este servicio

```bash
dotnet run --project Admin --launch-profile https
```

Requiere que **Auth** esté corriendo (para validar tokens). Para el reporte con IA, Ollama debe estar activo
con el modelo descargado: `ollama pull qwen3:8b`.

## Migraciones (EF Core Code First)

Las migraciones se aplican solas al iniciar (`Database.Migrate()` en `Program.cs`). Para crear una nueva:

```bash
dotnet ef migrations add NombreMigracion --project Domain --startup-project Admin
```

## Docker

```bash
docker build -t supermercado-admin .
```

Normalmente se levanta junto al resto con el `docker-compose.yml` del repo del Gateway.

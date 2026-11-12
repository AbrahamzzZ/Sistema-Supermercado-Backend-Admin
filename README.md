# Sistema Supermercado — Admin

Microservicio de **datos maestros y reportes** del supermercado: productos, categorías, clientes, proveedores,
transportistas, sucursales, ofertas y datos del negocio.

> 📘 Para levantar **todo el sistema** (Docker, Kafka, Gateway, Seq, etc.) ver el
> [README principal en el repo del Gateway](https://github.com/AbrahamzzZ/Sistema-Supermercado-Backend-Gateway#readme).

## Responsabilidad

- CRUD de los datos maestros que usan el resto de servicios
- Reportes del negocio (productos más vendidos/comprados, top clientes y proveedores, etc.) — ver [Reportes del negocio](#reportes-del-negocio)
- Asistente de IA sobre los datos del negocio usando **Ollama** (`qwen3:8b`)
- Ubicación geográfica de sucursales (tipo `geography` de SQL Server con NetTopologySuite)

**Base de datos:** `DB_Admin` — tablas `PRODUCTO`, `CATEGORIA`, `CLIENTE`, `PROVEEDOR`, `TRANSPORTISTA`,
`SUCURSAL`, `OFERTA`, `NEGOCIO`.

## Estructura

```
Admin/                    API (controllers, Program.cs, appsettings)
  Middleware/             CorrelationIdMiddleware y GlobalExceptionHandler
Domain/                   Entidades, DTOs, DbContext, configuraciones EF, Mapper y Migrations
  Model/Common/           EntidadAuditable e IEliminable (auditoría y borrado lógico)
  Migrations/Scripts/     Stored procedures y datos iniciales (.sql embebidos)
Infrastructure/           Repositorios, servicios y validadores (FluentValidation)
  Security/               ICurrentUserService (usuario del token para la auditoría)
  Handler/                TokenDelegatingHandler (reenvía token y X-Correlation-Id a otros microservicios)
  Services/               Servicios y MicroserviciosApiClient (Order, Inventory y Auth vía Gateway)
Utilities/                ApiResponse, mensajes, paginación, cliente de Ollama y reglas del asistente de IA
```

## Endpoints

Vía Gateway: `http://localhost:8000/admin/...` → aquí `/api/...` · **Todos requieren token.**

Cada uno de estos recursos tiene el mismo CRUD:
`Producto`, `Categoria`, `Cliente`, `Proveedor`, `Transportista`, `Sucursal`, `Oferta`

| Método | Ruta | Descripción |
|---|---|---|
| GET | `/api/{Recurso}` | Lista los registros **no eliminados** |
| GET | `/api/{Recurso}/paginacion?pageNumber=1&pageSize=10` | Lista paginada |
| GET | `/api/{Recurso}/{id}` | Obtiene uno (404 si no existe o fue eliminado) |
| POST | `/api/{Recurso}` | Registra |
| PUT | `/api/{Recurso}/{id}` | Edita |
| DELETE | `/api/{Recurso}/{id}` | Elimina (**borrado lógico**) |

Negocio y reportes (mismas rutas que el monolito, las usa el frontend):

| Método | Ruta | Datos |
|---|---|---|
| GET / PUT | `/api/Negocio/{id}` | `DB_Admin` |
| GET | `/api/Negocio/productos-mas-comprados` | Top 5 · Order + nombres de `DB_Admin` |
| GET | `/api/Negocio/productos-mas-vendidos` | Top 5 · Order + nombres de `DB_Admin` |
| GET | `/api/Negocio/top-clientes` | Top 5 · Order + nombres de `DB_Admin` |
| GET | `/api/Negocio/top-proveedores` | Top 5 · Order + nombres de `DB_Admin` |
| GET | `/api/Negocio/viajes-transportista` | Top 5 · Order + nombres de `DB_Admin` |
| GET | `/api/Negocio/empleados-productivos` | Top 5 · Order + nombres de Auth |
| POST | `/api/Negocio/analisis-ia` | Body `{ "prompt": "..." }` · usa Ollama |

Si todavía no hay compras o ventas, los reportes responden `404` con `"No se encontraron registros."`.

Las respuestas incluyen ahora los campos de auditoría `idUsuarioCreacion`, `fechaCreacion`,
`idUsuarioModificacion` y `fechaModificacion`. Los nombres antiguos (`fecha_Registro`, `fecha_Creacion`) se
siguen enviando por compatibilidad con el front.

Swagger: http://localhost:8001/swagger (Docker) · https://localhost:7047/swagger (local)

## Quién consume este servicio

**Order** e **Inventory** llaman a `GET /admin/{Recurso}/{id}` **a través del Gateway** para validar que un
producto, cliente, proveedor, sucursal o transportista existe y para mostrar sus datos.
Un registro eliminado (lógicamente) responde 404, así que ya no se puede usar en compras o ventas nuevas.

## Reportes del negocio

Las compras y ventas viven en `DB_Order` y los nombres en `DB_Admin` (y en `DB_Auth` los empleados), así que un
reporte no se puede resolver con un solo `JOIN` como en el monolito. Cada microservicio aporta lo suyo:

```
Front ──► Gateway ──► Admin  GET /api/Negocio/top-clientes
                        │
                        ├─► Gateway ──► Order  GET /api/Estadistica/top-clientes   →  [{ id: 3, total: 12 }, ...]
                        │
                        └─► DB_Admin: nombres de esos IDs                          →  [{ nombre_Completo: "Ana Ruiz", compras_Totales: 12 }]
```

- **Order** (`/api/Estadistica/...`) devuelve solo **IDs y totales** (top 5), calculados con los SP
  `PA_ESTADISTICA_*` sobre sus tablas.
- **Admin** busca los nombres de esos IDs en `DB_Admin` en una sola consulta, **incluidos los registros
  eliminados** (un reporte histórico debe mostrar el nombre de un cliente aunque ya se haya eliminado). Si un ID
  no existe, muestra `Cliente #ID`.
- **Empleados productivos:** los empleados son usuarios de Auth; Admin consulta `GET /auth/Usuario/{id}` por
  cada uno (máximo 5).
- Las llamadas pasan por el Gateway (`Services:GatewayUrl`) reenviando el token del usuario y el
  `X-Correlation-Id` (`Infrastructure/Handler/TokenDelegatingHandler.cs`), así en Seq se ve el recorrido completo.
- Si Order no responde, el reporte devuelve `"No se pudieron obtener los datos de otro servicio..."` y se
  registra como `Error`.

### Asistente de IA (`POST /api/Negocio/analisis-ia`)

Mismas reglas que el monolito (`Utilities/IA/Reglas.cs`): según las palabras de la pregunta se eligen los datos
que se envían a Ollama:

| La pregunta habla de… | Datos que recibe la IA | Origen |
|---|---|---|
| productos, artículos | Productos y su stock | `DB_Admin` + Inventory (`GET /inventory/ProductoStock`) |
| categorías, secciones | Categorías | `DB_Admin` |
| clientes | Top clientes | Order + `DB_Admin` |
| proveedores | Top proveedores | Order + `DB_Admin` |
| ventas, vendidos | Unidades vendidas por producto y día | Order + `DB_Admin` |
| compras, comprados | Unidades compradas por producto y día | Order + `DB_Admin` |
| otra cosa | — | Responde `"La pregunta no está relacionada con nada del negocio."` |

Si Ollama no está disponible responde `"El servicio de IA no está disponible en este momento."` (log `Error`).

## Auditoría y borrado lógico

Todas las tablas tienen las columnas de auditoría (mismo contrato en los 4 microservicios):

| Columna | Tipo | Quién la llena |
|---|---|---|
| `ID_USUARIO_CREACION` | `INT NULL` | El SP de registrar (`@Id_Usuario_Creacion`) |
| `FECHA_CREACION` | `DATETIME2 NOT NULL` | SQL Server: `DEFAULT SYSUTCDATETIME()` |
| `ID_USUARIO_MODIFICACION` | `INT NULL` | Los SP de editar y eliminar (`@Id_Usuario_Modificacion`) |
| `FECHA_MODIFICACION` | `DATETIME2 NULL` | Los SP de editar y eliminar: `SYSUTCDATETIME()` |

- **El usuario sale del token JWT** (claim `NameIdentifier`) mediante `ICurrentUserService`; nunca del body.
  Los repositorios lo pasan a los stored procedures.
- **Todas las fechas se guardan en UTC.** El front debe convertirlas a la hora local.
- No hay FK hacia el usuario: el usuario vive en otra base (`DB_Auth`, microservicio Auth).
- En `CLIENTE`, `PROVEEDOR` y `TRANSPORTISTA` la columna `FECHA_REGISTRO` se **renombró** a `FECHA_CREACION`
  (los datos existentes se conservan).

**Borrado lógico:** todas las tablas menos `NEGOCIO` tienen `ELIMINADO BIT NOT NULL DEFAULT 0`.

- `DELETE` hace `UPDATE ... SET ELIMINADO = 1` y registra quién y cuándo lo eliminó.
- Los SP de listar, paginar y obtener filtran `ELIMINADO = 0`.
- Así no se pierden datos que otros microservicios referencian (por ejemplo, un cliente de una venta en Order).
- Como la FK ya no impide borrar, los SP validan las dependencias y lanzan el error **50547** (el servicio
  lo trata igual que el antiguo error 547 de FK):
  - una categoría con productos activos no se puede eliminar;
  - un producto con ofertas activas no se puede eliminar.

## Logs (Serilog + Seq)

Los logs son estructurados y se envían a la consola y a **Seq** (`Seq:ServerUrl`; en Docker `http://seq:80`).
Todos llevan las propiedades `Servicio = Admin` y `CorrelationId`.

| Nivel | Cuándo |
|---|---|
| `Information` | La operación salió bien (incluye un log por cada petición HTTP 2xx) |
| `Warning` | Se incumplió una regla de negocio o validación: código duplicado, no encontrado, dependencias, etc. (y peticiones 4xx) |
| `Error` | Fallo técnico: excepción no controlada, base de datos caída, Ollama no disponible (y peticiones 5xx) |

- **`X-Correlation-Id`:** si la petición trae este header (lo agrega el Gateway) se reutiliza; si no, se genera.
  Se devuelve en la respuesta y viaja en todos los logs, así puedes seguir una operación entre microservicios en Seq.
- **Errores no controlados:** `GlobalExceptionHandler` registra la excepción y devuelve un `500` con el formato
  `ApiResponse` y el mensaje `Ocurrió un error interno. Código de seguimiento: {correlationId}`. Con ese código
  se busca el error en Seq.
- El log de cada petición incluye `IdUsuario`, método, ruta, código de estado y duración.

## Configuración

| Clave | Descripción |
|---|---|
| `ConnectionStrings:CadenaSQL` | Cadena de conexión a `DB_Admin` |
| `Jwt:Authority` | URL de Auth, de donde se descarga la clave pública (local: `https://localhost:7186`) |
| `Jwt:RequireHttpsMetadata` | `false` solo dentro de Docker (http) |
| `Jwt:Issuer` / `Jwt:Audience` | Deben coincidir con los de Auth |
| `Ollama:BaseUrl` | URL de Ollama (local: `http://localhost:11434`; Docker: `http://host.docker.internal:11434`) |
| `Services:GatewayUrl` | URL del Gateway, para los reportes (local: `https://localhost:7134`; Docker: `http://gateway:8080`) |
| `Seq:ServerUrl` | URL de Seq (local: `http://localhost:5341`; Docker: `http://seq:80`) |
| `Serilog:MinimumLevel` | Nivel mínimo de log y overrides por categoría |

Este servicio **no tiene ninguna clave JWT**: solo valida tokens con la clave pública que publica Auth.

## Ejecutar solo este servicio

```bash
dotnet run --project Admin --launch-profile https
```

Requiere que **Auth** esté corriendo (para validar tokens). Los reportes necesitan además el **Gateway** y
**Order** (e **Inventory** para el análisis de productos con IA). Para el asistente de IA, Ollama debe estar
activo con el modelo descargado: `ollama pull qwen3:8b`. Para ver los logs en Seq localmente, levanta el contenedor
`seq` desde el `docker-compose.yml` del Gateway.

## Migraciones (EF Core Code First)

Las migraciones se aplican solas al iniciar (`Database.Migrate()` en `Program.cs`).

| Migración | Contenido |
|---|---|
| `Initial` | Tablas |
| `FixSucursalUbicacion` | Columna `geography` de sucursal |
| `AuditoriaYProcedimientos` | Columnas de auditoría y `ELIMINADO`, **stored procedures** y **datos iniciales** |

### Stored procedures dentro de las migraciones

Los SP no se crean a mano: viven como archivos `.sql` en `Domain/Migrations/Scripts/`, marcados como
`EmbeddedResource` en `Domain.csproj`. La migración los ejecuta con el helper `ScriptRunner`:

```csharp
migrationBuilder.EjecutarScript("V1_07_Producto.sql");
```

`ScriptRunner` lee el recurso embebido y lo divide por las líneas `GO` (como SSMS), porque cada
`CREATE PROCEDURE` debe ir en su propio lote. Los SP usan `CREATE OR ALTER`.

| Archivo | Contenido |
|---|---|
| `V1_01_Negocio.sql` … `V1_08_Oferta.sql` | SP de cada módulo |
| `V1_99_DatosIniciales.sql` | Negocio (id 1), 2 sucursales y 14 categorías. Es idempotente (`IF NOT EXISTS`) |

### Cómo cambiar un SP en el futuro

**Nunca edites un script que ya forma parte de una migración aplicada** (las bases que ya la aplicaron no
volverán a ejecutarlo). En su lugar:

1. Crea un archivo nuevo con la siguiente versión, por ejemplo `Domain/Migrations/Scripts/V2_07_Producto.sql`,
   con el `CREATE OR ALTER PROCEDURE` completo.
2. Crea una migración: `dotnet ef migrations add ActualizarSpProducto --project Domain --startup-project Admin`
3. En su `Up()` llama a `migrationBuilder.EjecutarScript("V2_07_Producto.sql");` y en `Down()` vuelve a ejecutar
   la versión anterior (`V1_07_Producto.sql`).

Para cambios de tablas, modifica las entidades y su configuración y genera la migración como siempre:

```bash
dotnet ef migrations add NombreMigracion --project Domain --startup-project Admin
```

## Docker

```bash
docker build -t supermercado-admin .
```

Normalmente se levanta junto al resto con el `docker-compose.yml` del repo del Gateway.

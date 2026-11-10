# Sistema Supermercado — Admin

Microservicio de **datos maestros y reportes** del supermercado: productos, categorías, clientes, proveedores,
transportistas, sucursales, ofertas y datos del negocio.

> 📘 Para levantar **todo el sistema** (Docker, Kafka, Gateway, Seq, etc.) ver el
> [README principal en el repo del Gateway](https://github.com/AbrahamzzZ/Sistema-Supermercado-Backend-Gateway#readme).

## Responsabilidad

- CRUD de los datos maestros que usan el resto de servicios
- Reportes del negocio (productos más vendidos/comprados, top clientes y proveedores, etc.) — ver [Pendientes](#pendientes)
- Análisis con IA de los productos más vendidos usando **Ollama** (`qwen3:8b`)
- Ubicación geográfica de sucursales (tipo `geography` de SQL Server con NetTopologySuite)

**Base de datos:** `DB_Admin` — tablas `PRODUCTO`, `CATEGORIA`, `CLIENTE`, `PROVEEDOR`, `TRANSPORTISTA`,
`SUCURSAL`, `OFERTA`, `NEGOCIO`.

## Estructura

```
Admin/                    API (controllers, Program.cs, appsettings)
  Observabilidad/         CorrelationIdMiddleware y GlobalExceptionHandler
Domain/                   Entidades, DTOs, DbContext, configuraciones EF, Mapper y Migrations
  Model/Common/           EntidadAuditable e IEliminable (auditoría y borrado lógico)
  Migrations/Scripts/     Stored procedures y datos iniciales (.sql embebidos)
Infrastructure/           Repositorios, servicios y validadores (FluentValidation)
  Security/               ICurrentUserService (usuario del token para la auditoría)
Utilities/                ApiResponse, mensajes, paginación y cliente de Ollama
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

Las respuestas incluyen ahora los campos de auditoría `idUsuarioCreacion`, `fechaCreacion`,
`idUsuarioModificacion` y `fechaModificacion`. Los nombres antiguos (`fecha_Registro`, `fecha_Creacion`) se
siguen enviando por compatibilidad con el front.

Swagger: http://localhost:8001/swagger (Docker) · https://localhost:7047/swagger (local)

## Quién consume este servicio

**Order** e **Inventory** llaman a `GET /admin/{Recurso}/{id}` **a través del Gateway** para validar que un
producto, cliente, proveedor, sucursal o transportista existe y para mostrar sus datos.
Un registro eliminado (lógicamente) responde 404, así que ya no se puede usar en compras o ventas nuevas.

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
| `Seq:ServerUrl` | URL de Seq (local: `http://localhost:5341`; Docker: `http://seq:80`) |
| `Serilog:MinimumLevel` | Nivel mínimo de log y overrides por categoría |

Este servicio **no tiene ninguna clave JWT**: solo valida tokens con la clave pública que publica Auth.

## Ejecutar solo este servicio

```bash
dotnet run --project Admin --launch-profile https
```

Requiere que **Auth** esté corriendo (para validar tokens). Para el reporte con IA, Ollama debe estar activo
con el modelo descargado: `ollama pull qwen3:8b`. Para ver los logs en Seq localmente, levanta el contenedor
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

## Pendientes

- **Reportes de Negocio:** `PA_PRODUCTOS_MAS_COMPRADOS`, `PA_PRODUCTOS_MAS_VENDIDOS`, `PA_TOP_CLIENTES`,
  `PA_TOP_PROVEEDORES`, `PA_VIAJES_TRANSPORTISTA` y `PA_VENTAS_EMPLEADO` **no se crean en la migración**:
  necesitan datos de compras y ventas, que viven en `DB_Order` (otro microservicio) y no se pueden consultar
  desde `DB_Admin`. Mientras tanto, los endpoints de reportes de `/api/Negocio/...` responden con error.

## Docker

```bash
docker build -t supermercado-admin .
```

Normalmente se levanta junto al resto con el `docker-compose.yml` del repo del Gateway.

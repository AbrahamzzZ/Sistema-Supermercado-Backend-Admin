# Sistema Supermercado — Admin

Microservicio de **datos maestros y reportes**: productos, categorías, clientes, proveedores, transportistas,
sucursales, ofertas y datos del negocio. También arma los **reportes del negocio** (con datos de Order), atiende el
**asistente de IA** (OpenAI) y expone los **logs del sistema** que se ven en la app (leídos desde Seq).

**Base de datos:** `DB_Admin` — `PRODUCTO`, `CATEGORIA`, `CLIENTE`, `PROVEEDOR`, `TRANSPORTISTA`, `SUCURSAL`, `OFERTA`, `NEGOCIO`.

📘 Documentación:
- [Cómo levantar todo el sistema](https://github.com/AbrahamzzZ/Sistema-Supermercado-Backend-Gateway#readme) (README del Gateway)
- [Manual técnico](https://github.com/AbrahamzzZ/Sistema-Supermercado-Backend-Gateway/blob/main/docs/Manual%20Tecnico.docx): capítulo 7 (Admin), 11 (Reportes e IA) y 13 (Logs)

## Ejecutar

Normalmente se levanta con el `docker-compose.yml` del repo del Gateway (Swagger: http://localhost:8001/swagger).

**Para depurar (modo híbrido):** el resto del sistema sigue en Docker y este servicio corre desde Visual Studio.

```bash
# en el repo del Gateway
docker compose up -d                  # todo el sistema
docker compose stop admin-api       # libera el servicio que vas a depurar
```

Luego F5 en Visual Studio (o `dotnet run --project Admin --launch-profile https`) → https://localhost:7047/swagger.
En el entorno Development, `appsettings.Development.json` lo conecta a lo que corre en Docker: SQL Server
`localhost,14330` (las mismas bases), Auth `http://localhost:8002` (validación de tokens), Gateway
`http://localhost:8000`.

Pruébalo desde su Swagger con un token de `POST http://localhost:8000/auth/Usuario/login` (botón *Authorize*):
el frontend sigue pasando por el Gateway de Docker, que apunta al contenedor detenido. Al terminar:
`docker compose up -d --build admin-api`.

Para usar tu propio SQL Server en vez del de Docker, define `ConnectionStrings:CadenaSQL` con `dotnet user-secrets`
(tiene prioridad sobre `appsettings.Development.json`).

El asistente de IA usa la API de OpenAI. Para configurar la clave en desarrollo local:

```bash
dotnet user-secrets set "OpenAI:ApiKey" "<tu-api-key>" --project Admin/Admin.csproj
```

El modelo predeterminado es `gpt-6-luna` y puede cambiarse con `OpenAI:Model`. Para Docker, configura
`OPENAI_API_KEY` en el entorno o en un archivo `.env` local junto al `docker-compose.yml` (no lo subas a Git).

## Endpoints principales

| Ruta (vía Gateway `/admin/...`) | Descripción |
|---|---|
| `/api/{Recurso}` | CRUD de Producto, Categoria, Cliente, Proveedor, Transportista, Sucursal y Oferta (borrado lógico) |
| `/api/Negocio/{id}` | Datos del negocio |
| `GET /api/Negocio/productos-mas-vendidos`, `top-clientes`, … | Reportes (top 5) |
| `POST /api/Negocio/analisis-ia` | Asistente de IA: responde preguntas sobre el negocio |
| `POST /api/Negocio/resumen-grafico-ia` | Asistente de IA: resume la estadística que se ve en el panel |
| `GET /api/Log/paginacion` | Errores y advertencias de todo el sistema (solo Administrador) |

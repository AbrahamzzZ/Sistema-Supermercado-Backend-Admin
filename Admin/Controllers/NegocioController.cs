using Domain.Model;
using Domain.Model.Dto.Negocio;
using Infrastructure.Repository.InterfacesServices;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Utilities.Shared;

namespace APIRestSistemaVentas.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class NegocioController : ControllerBase
    {
        private readonly NegocioService _negocioService;

        public NegocioController(NegocioService negocioService)
        {
            _negocioService = negocioService;
        }

        //Para pruebas unitarias, descomenta este constructor y comenta el constructor anterior.

        /*private readonly INegocioService _negocioService;

        public NegocioController(INegocioService negocioService)
        {
            _negocioService = negocioService;
        }*/

        // GET: api/negocio/1
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<Negocio>>> GetNegocio(int id)
        {
            var response = await _negocioService.ObtenerNegocioAsync(id);
            return response.IsSuccess ? Ok(response) : NotFound(response);
        }

        // PUT: api/negocio/1
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<object>>> EditarNegocio(int id, [FromBody] Negocio negocio)
        {
            var response = await _negocioService.EditarNegocioAsync(negocio);
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }

        // Reportes: mismas rutas que el monolito (las usa el frontend). Los totales vienen de Order y los nombres de DB_Admin.

        // GET: api/negocio/productos-mas-comprados
        [HttpGet("productos-mas-comprados")]
        public async Task<ActionResult<ApiResponse<List<ProductoMasComprado>>>> ObtenerProductosMasComprados()
        {
            var response = await _negocioService.ObtenerProductoMasComprado();
            return response.IsSuccess ? Ok(response) : NotFound(response);
        }

        // GET: api/negocio/productos-mas-vendidos
        [HttpGet("productos-mas-vendidos")]
        public async Task<ActionResult<ApiResponse<List<ProductoMasVendido>>>> ObtenerProductosMasVendidos()
        {
            var response = await _negocioService.ObtenerProductoMasVendido();
            return response.IsSuccess ? Ok(response) : NotFound(response);
        }

        // POST: api/negocio/analisis-ia
        [HttpPost("analisis-ia")]
        public async Task<ActionResult<ApiResponse<string>>> GenerarAnalisisIA([FromBody] AnalisisIARequest request)
        {
            var response = await _negocioService.AnalisisIA(request.Prompt);
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }

        // GET: api/negocio/top-clientes
        [HttpGet("top-clientes")]
        public async Task<ActionResult<ApiResponse<List<TopCliente>>>> ObtenerTopClientes()
        {
            var response = await _negocioService.ObtenerTopClientes();
            return response.IsSuccess ? Ok(response) : NotFound(response);
        }

        // GET: api/negocio/top-proveedores
        [HttpGet("top-proveedores")]
        public async Task<ActionResult<ApiResponse<List<TopProveedor>>>> ObtenerProveedorPreferido()
        {
            var response = await _negocioService.ObtenerTopProveedores();
            return response.IsSuccess ? Ok(response) : NotFound(response);
        }

        // GET: api/negocio/viaje-transportista
        [HttpGet("viajes-transportista")]
        public async Task<ActionResult<ApiResponse<List<ViajesTransportista>>>> ObtenerTransportistaViajes()
        {
            var response = await _negocioService.ObtenerViajesTransportista();
            return response.IsSuccess ? Ok(response) : NotFound(response);
        }

        // GET: api/negocio/empleados-productivos
        [HttpGet("empleados-productivos")]
        public async Task<ActionResult<ApiResponse<List<EmpleadoProductivo>>>> ObtenerEmpleadosProductivos()
        {
            var response = await _negocioService.ObtenerEmpleadosProductivos();
            return response.IsSuccess ? Ok(response) : NotFound(response);
        }
    }
}

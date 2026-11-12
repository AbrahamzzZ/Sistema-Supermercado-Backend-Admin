using Domain.Model.Dto;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Utilities.Shared;

namespace APIRestSistemaVentas.Controllers
{
    [Authorize(Roles = "Administrador")]
    [Route("api/[controller]")]
    [ApiController]
    public class LogController : ControllerBase
    {
        private readonly SeqLogService _seqLogService;

        public LogController(SeqLogService seqLogService)
        {
            _seqLogService = seqLogService;
        }

        // GET: api/log/paginacion?pageNumber=1&pageSize=10&filtro=&desde=2026-11-01&hasta=2026-11-12
        [HttpGet("paginacion")]
        public async Task<ActionResult<ApiResponse<Paginacion<LogSistema>>>> GetLogsPaginacion(
            int pageNumber = 1, int pageSize = 10, string filtro = "", DateOnly? desde = null, DateOnly? hasta = null)
        {
            if (desde.HasValue && hasta.HasValue && desde.Value > hasta.Value)
                return BadRequest(new ApiResponse<Paginacion<LogSistema>> { IsSuccess = false, Message = "La fecha 'desde' no puede ser posterior a la fecha 'hasta'." });

            var response = await _seqLogService.ListarPaginacionAsync(pageNumber, pageSize, filtro, desde, hasta);
            return response.IsSuccess ? Ok(response) : StatusCode(StatusCodes.Status503ServiceUnavailable, response);
        }

        // GET: api/log/event-24f6399c40d308dfba2a000000000000
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<LogSistema>>> GetLog(string id)
        {
            var response = await _seqLogService.ObtenerAsync(id);
            return response.IsSuccess ? Ok(response) : NotFound(response);
        }
    }
}

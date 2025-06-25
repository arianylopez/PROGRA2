using GestionEventos.API.Logica;
using Microsoft.AspNetCore.Mvc;

namespace GestionEventos.API.Controllers
{
    [ApiController]
    [Route("api/dashboard")]
    public class DashboardController : ControllerBase
    {
        private readonly IConfiguration _config;
        public DashboardController(IConfiguration config) { _config = config; }

        [HttpGet("evento/{eventoId}")]
        public IActionResult GetDashboardEvento(int eventoId, [FromQuery] int organizadorId)
        {
            if (organizadorId <= 0)
            {
                return Unauthorized("Se requiere un ID de organizador válido.");
            }

            var logica = new DashboardLogica(_config);
            var dashboardData = logica.GetDashboardEvento(eventoId, organizadorId);

            if (dashboardData == null)
            {
                return NotFound("No se encontraron datos del dashboard para este evento, o no tienes permiso para verlo.");
            }

            return Ok(dashboardData);
        }

        [HttpGet("competencia/{competenciaId}")]
        public IActionResult GetDashboardCompetencia(int competenciaId, [FromQuery] int organizadorId)
        {
            if (organizadorId <= 0)
            {
                return Unauthorized("Se requiere un ID de organizador válido.");
            }

            var logica = new DashboardLogica(_config);
            var dashboardData = logica.GetDashboardCompetencia(competenciaId, organizadorId);

            if (dashboardData == null)
            {
                return NotFound("No se encontraron datos del dashboard para esta competencia, o no tienes permiso para verlo.");
            }

            return Ok(dashboardData);
        }
    }
}
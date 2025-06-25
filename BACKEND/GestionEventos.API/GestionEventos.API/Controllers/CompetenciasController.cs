using GestionEventos.API.Logica;
using GestionEventos.API.Modelos.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace GestionEventos.API.Controllers
{
    [ApiController]
    [Route("api/competencias")]
    public class CompetenciasController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        public CompetenciasController(IConfiguration configuration) { _configuration = configuration; }

        [HttpGet]
        public IActionResult ListarCompetencias([FromQuery] int? categoriaId)
        {
            var logica = new CompetenciaLogica(_configuration);
            var lista = logica.ListarCompetenciasPublicas(categoriaId);
            return Ok(lista);
        }

        [HttpGet("{id}")]
        public IActionResult ObtenerCompetenciaPorId(int id)
        {
            var logica = new CompetenciaLogica(_configuration);
            var competencia = logica.ObtenerCompetenciaPorId(id);
            if (competencia == null) return NotFound("Competencia no encontrada.");
            return Ok(competencia);
        }

        [HttpPost]
        public IActionResult CrearCompetencia([FromBody] CrearCompetenciaRequest request)
        {
            var logica = new CompetenciaLogica(_configuration);
            var (exito, mensaje, competenciaCreada) = logica.CrearCompetencia(request);
            if (!exito)
            {
                return BadRequest(new { Mensaje = mensaje });
            }
            return Ok(competenciaCreada);
        }

        [HttpPut("{id}")]
        public IActionResult ActualizarCompetencia(int id, [FromBody] ActualizarCompetenciaRequest request)
        {
            if (id != request.CompetenciaID) return BadRequest("El ID de la ruta no coincide con el ID de la competencia.");
            var logica = new CompetenciaLogica(_configuration);
            var exito = logica.ActualizarCompetencia(request);
            if (!exito) return BadRequest("No se pudo actualizar la competencia. Verifique los datos o sus permisos.");
            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult EliminarCompetencia(int id, [FromQuery] int usuarioId)
        {
            var logica = new CompetenciaLogica(_configuration);
            var (exito, mensaje) = logica.EliminarCompetencia(id, usuarioId);

            if (!exito)
            {
                return BadRequest(new { Mensaje = mensaje });
            }

            return Ok(new { Mensaje = mensaje });
        }

        [HttpGet("organizador/{organizadorId}")]
        public IActionResult ListarPorOrganizador(int organizadorId)
        {
            var logica = new CompetenciaLogica(_configuration);
            var competencias = logica.ListarCompetenciasPorOrganizador(organizadorId);
            return Ok(competencias);
        }
    }
}
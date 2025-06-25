using GestionEventos.API.Logica;
using GestionEventos.API.Modelos;
using Microsoft.AspNetCore.Mvc;

namespace GestionEventos.API.Controllers
{
    [ApiController]
    [Route("api/resenas")]
    public class ResenasController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public ResenasController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet("evento/{eventoId}")]
        public async Task<IActionResult> ObtenerResenas(int eventoId)
        {
            var logica = new ResenaLogica(_configuration);
            var resenas = await logica.ObtenerResenasPorEvento(eventoId);
            return Ok(resenas);
        }

        [HttpPost]
        public async Task<IActionResult> CrearResena([FromBody] Resena nuevaResena)
        {
            var logica = new ResenaLogica(_configuration);
            var (exito, mensaje) = await logica.CrearResena(nuevaResena);

            if (!exito)
            {
                return BadRequest(new { Mensaje = mensaje });
            }
            return Ok(nuevaResena);
        }
    }
}
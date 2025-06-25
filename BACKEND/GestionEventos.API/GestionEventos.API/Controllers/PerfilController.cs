using GestionEventos.API.Logica;
using GestionEventos.API.Modelos.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace GestionEventos.API.Controllers
{
    [ApiController]
    [Route("api/perfil")]
    public class PerfilController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        public PerfilController(IConfiguration configuration) { _configuration = configuration; }

        [HttpGet("{usuarioId}/inscripciones")]
        public IActionResult GetMisInscripciones(int usuarioId)
        {
            var logica = new PerfilLogica(_configuration);
            var inscripciones = logica.ObtenerMisInscripciones(usuarioId);
            return Ok(inscripciones);
        }

        [HttpPost("cambiar-password")]
        public IActionResult CambiarPassword([FromBody] CambiarPasswordRequest request)
        {
            var logica = new PerfilLogica(_configuration);
            var (exito, mensaje) = logica.CambiarPassword(request);
            if (!exito)
            {
                return BadRequest(new { Mensaje = mensaje });
            }
            return Ok(new { Mensaje = mensaje });
        }

        [HttpPut("{usuarioId}")]
        public IActionResult ActualizarPerfil(int usuarioId, [FromBody] ActualizarPerfilRequest request)
        {
            var logica = new PerfilLogica(_configuration);
            var (exito, mensaje) = logica.ActualizarPerfil(usuarioId, request);

            if (!exito)
            {
                return BadRequest(new { Mensaje = mensaje });
            }

            return Ok(new { Mensaje = mensaje });
        }
    }
}
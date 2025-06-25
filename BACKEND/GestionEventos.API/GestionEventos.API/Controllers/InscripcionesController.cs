using GestionEventos.API.Logica;
using GestionEventos.API.Modelos.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace GestionEventos.API.Controllers
{
    [ApiController]
    [Route("api/inscripciones")]
    public class InscripcionesController : ControllerBase
    {
        private readonly IConfiguration _config;
        public InscripcionesController(IConfiguration config) { 
            _config = config; 
        }

        [HttpPost]
        public IActionResult ProcesarInscripcion([FromBody] InscripcionRequest request)
        {
            var logica = new InscripcionLogica(_config);
            var (exito, mensaje, resultado) = logica.ProcesarInscripcion(request);

            if (!exito)
            {
                return BadRequest(new { Mensaje = mensaje });
            }

            if (resultado != null)
            {
                return Ok(resultado);
            }

            return Ok(new { Mensaje = mensaje });
        }
    }
}

using GestionEventos.API.Logica;
using GestionEventos.API.Modelos;
using Microsoft.AspNetCore.Mvc;

namespace GestionEventos.API.Controllers
{
    [ApiController]
    [Route("api/preguntas-respuestas")] 
    public class PreguntasRespuestasController : ControllerBase 
    {
        private readonly IConfiguration _config;
        public PreguntasRespuestasController(IConfiguration config) { _config = config; }

        [HttpGet("{tipoActividad}/{actividadId}")]
        public async Task<IActionResult> Get(string tipoActividad, int actividadId)
        {
            var logica = new PreguntasRespuestasLogica(_config);
            return Ok(await logica.GetByActividadId(tipoActividad, actividadId));
        }

        [HttpPost("pregunta")]
        public async Task<IActionResult> HacerPregunta([FromBody] QnADocument pregunta)
        {
            var logica = new PreguntasRespuestasLogica(_config);
            await logica.HacerPregunta(pregunta);
            return Ok(pregunta);
        }

        [HttpPost("respuesta/{preguntaId}")]
        public async Task<IActionResult> Responder(string preguntaId, [FromBody] Respuesta respuesta)
        {
            var logica = new PreguntasRespuestasLogica(_config);
            var exito = await logica.ResponderPregunta(preguntaId, respuesta);
            if (!exito) return Unauthorized("No tienes permiso para responder o la pregunta no existe.");
            return Ok();
        }
    }
}
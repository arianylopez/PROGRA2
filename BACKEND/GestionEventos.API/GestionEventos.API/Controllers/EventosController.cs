using GestionEventos.API.Logica;
using GestionEventos.API.Modelos.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace GestionEventos.API.Controllers
{
    [ApiController]
    [Route("api/eventos")]
    public class EventosController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        public EventosController(IConfiguration configuration) { 
            _configuration = configuration; 
        }

        [HttpGet]
        public IActionResult ListarEventos([FromQuery] int? categoriaId)
        {
            var logica = new EventoLogica(_configuration);
            var lista = logica.ListarEventosPublicos(categoriaId);
            return Ok(lista);
        }

        [HttpGet("{id}")]
        public IActionResult ObtenerEventoPorId(int id)
        {
            var logica = new EventoLogica(_configuration);
            var evento = logica.ObtenerEventoPorId(id);
            if (evento == null) return NotFound("El evento no fue encontrado.");
            return Ok(evento);
        }

        [HttpPost]
        public IActionResult CrearEvento([FromBody] CrearEventoRequest request)
        {
            var logica = new EventoLogica(_configuration);
            var (exito, mensaje, eventoCreado) = logica.CrearEvento(request);
            if (!exito)
            {
                return BadRequest(new { Mensaje = mensaje });
            }
            return Ok(eventoCreado);
        }

        [HttpPut("{id}")]
        public IActionResult ActualizarEvento(int id, [FromBody] ActualizarEventoRequest request)
        {
            if (id != request.EventoID) return BadRequest("El ID de la ruta no coincide con el ID del evento.");
            var logica = new EventoLogica(_configuration);
            var exito = logica.ActualizarEvento(request);
            if (!exito) return BadRequest("No se pudo actualizar el evento. Verifique los datos o sus permisos.");
            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult EliminarEvento(int id, [FromQuery] int usuarioId)
        {
            var logica = new EventoLogica(_configuration);
            var (exito, mensaje) = logica.EliminarEvento(id, usuarioId);

            if (!exito)
            {
                return BadRequest(new { Mensaje = mensaje });
            }

            return Ok(new { Mensaje = mensaje });
        }

        [HttpGet("organizador/{organizadorId}")]
        public IActionResult ListarPorOrganizador(int organizadorId)
        {
            var logica = new EventoLogica(_configuration);
            var eventos = logica.ListarEventosPorOrganizador(organizadorId);
            return Ok(eventos);
        }
    }
}
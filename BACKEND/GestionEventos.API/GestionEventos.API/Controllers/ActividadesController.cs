using GestionEventos.API.Logica;
using Microsoft.AspNetCore.Mvc;

namespace GestionEventos.API.Controllers
{
    [ApiController]
    [Route("api/actividades")]
    public class ActividadesController : ControllerBase
    {
        private readonly IConfiguration _config;
        public ActividadesController(IConfiguration config) { _config = config; }

        [HttpGet("buscar")]
        public IActionResult Buscar([FromQuery] string? texto, [FromQuery] int? categoriaId, [FromQuery] string? tipoPrecio)
        {
            var logica = new ActividadLogica(_config);
            var resultados = logica.Buscar(texto, categoriaId, tipoPrecio);
            return Ok(resultados);
        }
    }
}
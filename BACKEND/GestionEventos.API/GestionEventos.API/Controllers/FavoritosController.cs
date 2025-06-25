using GestionEventos.API.Logica;
using GestionEventos.API.Modelos.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace GestionEventos.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FavoritosController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        public FavoritosController(IConfiguration configuration) { _configuration = configuration; }

        [HttpPost("evento")]
        public IActionResult AgregarFavoritoEvento([FromBody] FavoritoRequest request)
        {
            var logica = new FavoritosLogica(_configuration);
            var exito = logica.AgregarFavoritoEvento(request.UsuarioID, request.ItemID);
            return exito ? Ok() : BadRequest("El favorito ya existe o hubo un error.");
        }

        [HttpDelete("evento/{eventoId}")]
        public IActionResult EliminarFavoritoEvento(int eventoId, [FromQuery] int usuarioId)
        {
            var logica = new FavoritosLogica(_configuration);
            var exito = logica.EliminarFavoritoEvento(usuarioId, eventoId);
            return exito ? NoContent() : NotFound();
        }

        [HttpPost("competencia")]
        public IActionResult AgregarFavoritoCompetencia([FromBody] FavoritoRequest request)
        {
            var logica = new FavoritosLogica(_configuration);
            var exito = logica.AgregarFavoritoCompetencia(request.UsuarioID, request.ItemID);
            return exito ? Ok() : BadRequest();
        }

        [HttpDelete("competencia/{competenciaId}")]
        public IActionResult EliminarFavoritoCompetencia(int competenciaId, [FromQuery] int usuarioId)
        {
            var logica = new FavoritosLogica(_configuration);
            var exito = logica.EliminarFavoritoCompetencia(usuarioId, competenciaId);
            return exito ? NoContent() : NotFound();
        }


        [HttpGet("{usuarioId}")]
        public IActionResult GetFavoritos(int usuarioId)
        {
            var logica = new FavoritosLogica(_configuration);
            var favoritos = logica.ListarFavoritosPorUsuario(usuarioId);
            return Ok(favoritos);
        }
    }
}
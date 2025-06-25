using GestionEventos.API.Logica;
using Microsoft.AspNetCore.Mvc;

namespace GestionEventos.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriasController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        public CategoriasController(IConfiguration configuration) { _configuration = configuration; }

        [HttpGet]
        public IActionResult GetAll()
        {
            var logica = new CategoriaLogica(_configuration);
            var lista = logica.ListarCategorias();
            return Ok(lista);
        }
    }
}
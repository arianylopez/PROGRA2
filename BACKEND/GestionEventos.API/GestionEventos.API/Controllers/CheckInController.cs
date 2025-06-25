using GestionEventos.API.Logica;
using GestionEventos.API.Modelos.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace GestionEventos.API.Controllers
{
    [ApiController]
    [Route("api/checkin")]
    public class CheckInController : ControllerBase
    {
        private readonly IConfiguration _config;
        public CheckInController(IConfiguration config) { _config = config; }

        [HttpPost("validar")]
        public IActionResult Validar([FromBody] CheckInRequest request)
        {
            var logica = new CheckInLogica(_config);
            var (exito, mensaje) = logica.ValidarCodigo(request);

            if (!exito)
            {
                return Conflict(new { Mensaje = mensaje });
            }
            return Ok(new { Mensaje = mensaje });
        }
    }
}
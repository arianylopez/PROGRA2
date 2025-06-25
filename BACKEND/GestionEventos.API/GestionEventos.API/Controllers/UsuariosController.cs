using GestionEventos.API.Logica;
using GestionEventos.API.Modelos;
using GestionEventos.API.Modelos.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace GestionEventos.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuariosController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        public UsuariosController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public class LoginRequest
        {
            public string Email { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
        }

        [HttpGet("preguntas")]
        public IActionResult ListarPreguntas()
        {
            var logica = new UsuarioLogica(_configuration);
            return Ok(logica.ListarPreguntas());
        }

        [HttpPost("registro")]
        public IActionResult RegistrarUsuario([FromBody] Usuario nuevoUsuario)
        {
            var logica = new UsuarioLogica(_configuration);
            var (exito, mensaje, usuarioRegistrado) = logica.RegistrarUsuario(nuevoUsuario);

            if (!exito)
            {
                return BadRequest(new { Mensaje = mensaje });
            }
            return Ok(usuarioRegistrado);
        }

        [HttpPost("login")]
        public IActionResult IniciarSesion([FromBody] LoginRequest request)
        {
            var logica = new UsuarioLogica(_configuration);
            var usuario = logica.IniciarSesion(request.Email, request.Password);
            if (usuario == null)
            {
                return Unauthorized("Credenciales no válidas.");
            }
            return Ok(usuario);
        }

        [HttpPost("solicitar-recuperacion")]
        public IActionResult SolicitarRecuperacion([FromBody] SolicitarRecuperacionRequest request)
        {
            var logica = new UsuarioLogica(_configuration);
            var respuesta = logica.SolicitarRecuperacion(request.Email);
            if (respuesta == null)
            {
                return NotFound("Email no encontrado.");
            }
            return Ok(respuesta);
        }

        [HttpPost("confirmar-recuperacion")]
        public IActionResult ConfirmarRecuperacion([FromBody] ConfirmarRecuperacionRequest request)
        {
            var logica = new UsuarioLogica(_configuration);
            var exito = logica.ConfirmarRecuperacion(request);
            if (!exito)
            {
                return BadRequest("La respuesta de seguridad es incorrecta o el usuario no existe.");
            }
            return Ok(new { Mensaje = "Contraseña actualizada exitosamente." });
        }
    }
}
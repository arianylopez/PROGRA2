using GestionEventos.API.Datos;
using GestionEventos.API.Logica; // Se cambia la lógica que se usa
using GestionEventos.API.Modelos.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace GestionEventos.API.Controllers
{
    [ApiController]
    [Route("api/pagos")]
    public class PagosController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        public PagosController(IConfiguration configuration) { 
            _configuration = configuration; 
        }

        [HttpPost("confirmar")]
        public IActionResult ConfirmarPago([FromBody] ConfirmarPagoRequest request)
        {
            try
            {
                Console.WriteLine($"Request recibido: PagoID={request?.PagoID}, TransaccionID={request?.TransaccionSimuladaID}, Metodo={request?.MetodoPagoSimulado}");

                if (request == null)
                {
                    Console.WriteLine("Request es null");
                    return BadRequest(new { Mensaje = "Request no puede ser null" });
                }

                var logica = new PagoLogica(_configuration);
                var exito = logica.ConfirmarPago(request);

                Console.WriteLine($"Resultado de ConfirmarPago: {exito}");

                if (!exito)
                {
                    return BadRequest(new { Mensaje = "No se pudo confirmar el pago. El pago podría no existir, ya fue procesado o hubo un error." });
                }
                return Ok(new { Mensaje = "Pago confirmado exitosamente." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en ConfirmarPago: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                return BadRequest(new { Mensaje = $"Error interno: {ex.Message}" });
            }
        }

        [HttpGet("{pagoId}/detalle")]
        public IActionResult GetDetalle(int pagoId)
        {
            var pagoDAO = new PagoDAO(_configuration.GetConnectionString("SqlConnection") ?? "");
            var detalle = pagoDAO.ObtenerDetalleDelPago(pagoId);
            if (detalle == null)
            {
                return NotFound("No se encontraron detalles para este pago.");
            }
            return Ok(detalle);
        }
    }
}
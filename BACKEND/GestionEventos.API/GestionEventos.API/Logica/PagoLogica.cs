using GestionEventos.API.Datos;
using GestionEventos.API.Modelos.DTOs;

namespace GestionEventos.API.Logica
{
    public class PagoLogica
    {
        private readonly PagoDAO _pagoDAO;

        public PagoLogica(IConfiguration configuration)
        {
            var cadena = configuration.GetConnectionString("SqlConnection") ?? "";
            _pagoDAO = new PagoDAO(cadena);
        }

        public bool ConfirmarPago(ConfirmarPagoRequest request)
        {
            try
            {
                Console.WriteLine($"PagoLogica - Validando request: PagoID={request?.PagoID}, TransaccionID={request?.TransaccionSimuladaID}");

                if (request == null || request.PagoID <= 0 || string.IsNullOrEmpty(request.TransaccionSimuladaID))
                {
                    Console.WriteLine("Validación falló en PagoLogica");
                    return false;
                }

                var resultado = _pagoDAO.ConfirmarPago(request);
                Console.WriteLine($"Resultado del DAO: {resultado}");
                return resultado;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en PagoLogica.ConfirmarPago: {ex.Message}");
                return false;
            }
        }
    }
}
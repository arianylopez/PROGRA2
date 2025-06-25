namespace GestionEventos.API.Modelos.DTOs
{
    public class ConfirmarPagoRequest
    {
        public int PagoID { get; set; }
        public string MetodoPagoSimulado { get; set; } = string.Empty;
        public string TransaccionSimuladaID { get; set; } = string.Empty;
    }
}
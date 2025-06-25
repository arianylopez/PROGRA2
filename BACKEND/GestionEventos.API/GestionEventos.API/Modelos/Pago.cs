namespace GestionEventos.API.Modelos
{
    public class Pago
    {
        public int PagoID { get; set; }
        public int UsuarioID { get; set; }
        public decimal MontoTotal { get; set; }
        public DateTime FechaPago { get; set; }
        public string EstadoPago { get; set; } = string.Empty;
        public string? MetodoPagoSimulado { get; set; }
        public string? TransaccionID { get; set; }
    }
}
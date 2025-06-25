namespace GestionEventos.API.Modelos.DTOs
{
    public class PagoItemDTO
    {
        public string Descripcion { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }
    }
}
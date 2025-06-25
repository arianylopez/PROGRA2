namespace GestionEventos.API.Modelos.DTOs.Dashboard
{
    public class DashboardRenglonVentaDTO
    {
        public string TipoEntrada { get; set; } = string.Empty;
        public decimal Precio { get; set; }
        public int CantidadVendida { get; set; }
        public decimal Subtotal { get; set; }
    }
}
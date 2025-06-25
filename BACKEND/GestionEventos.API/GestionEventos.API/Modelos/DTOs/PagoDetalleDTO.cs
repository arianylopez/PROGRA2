namespace GestionEventos.API.Modelos.DTOs
{
    public class PagoDetalleDTO
    {
        public int PagoID { get; set; }
        public string NombreActividad { get; set; } = string.Empty;
        public decimal MontoTotal { get; set; }
        public List<PagoItemDTO> Items { get; set; } = new List<PagoItemDTO>();
    }
}
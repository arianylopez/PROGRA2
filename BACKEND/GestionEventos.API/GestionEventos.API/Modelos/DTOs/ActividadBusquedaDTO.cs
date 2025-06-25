namespace GestionEventos.API.Modelos.DTOs
{
    public class ActividadBusquedaDTO
    {
        public int ActividadID { get; set; }
        public string TipoActividad { get; set; } = string.Empty; 
        public string Titulo { get; set; } = string.Empty;
        public DateTime Fecha { get; set; } 
        public string? Ubicacion { get; set; }
        public decimal Precio { get; set; } 
        public string? ImagenUrl { get; set; }
    }
}
namespace GestionEventos.API.Modelos.DTOs
{
    public class EventoPublicoDTO
    {
        public int EventoID { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public DateTime FechaEvento { get; set; }
        public string Ubicacion { get; set; } = string.Empty;
        public string OrganizadorNombre { get; set; } = string.Empty;
        public string? Categorias { get; set; }
        public string? ImagenUrl { get; set; } 
        public string ModalidadPago { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
    }
}
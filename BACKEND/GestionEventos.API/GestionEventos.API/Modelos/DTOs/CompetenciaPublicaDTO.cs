namespace GestionEventos.API.Modelos.DTOs
{
    public class CompetenciaPublicaDTO
    {
        public int CompetenciaID { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public DateTime FechaInicio { get; set; } 
        public DateTime FechaFin { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public string OrganizadorNombre { get; set; } = string.Empty;
        public string ModalidadPago { get; set; } = string.Empty;
        public string? Categorias { get; set; }
        public string? ImagenUrl { get; set; }
        public string Estado { get; set; } = string.Empty;
    }
}
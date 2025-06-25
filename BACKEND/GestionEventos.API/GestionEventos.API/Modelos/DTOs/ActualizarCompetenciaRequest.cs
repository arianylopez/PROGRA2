namespace GestionEventos.API.Modelos.DTOs
{
    public class ActualizarCompetenciaRequest
    {
        public int CompetenciaID { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public int? TamanoMinEquipo { get; set; }
        public int? TamanoMaxEquipo { get; set; }
        public string? ImagenUrl { get; set; } 
        public int UsuarioIDQueActualiza { get; set; }
        public List<int> CategoriaIDs { get; set; } = new List<int>();
        public List<TiposEntradaCompetencia> Entradas { get; set; } = new List<TiposEntradaCompetencia>();
    }
}
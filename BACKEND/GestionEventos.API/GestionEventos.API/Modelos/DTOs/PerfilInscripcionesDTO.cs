namespace GestionEventos.API.Modelos.DTOs
{
    public class InscripcionEventoVista
    {
        public int InscripcionID { get; set; }
        public string TituloEvento { get; set; } = string.Empty;
        public DateTime FechaEvento { get; set; }
        public string Ubicacion { get; set; } = string.Empty;
        public string TipoEntrada { get; set; } = string.Empty;
        public decimal Precio { get; set; }
        public string CodigoCheckIn { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public int? PagoID { get; set; }
    }

    public class InscripcionCompetenciaVista
    {
        public int InscripcionID { get; set; }
        public string TituloCompetencia { get; set; } = string.Empty;
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public string? NombreEquipo { get; set; }
        public string TipoInscripcion { get; set; } = string.Empty;
        public decimal Precio { get; set; }
        public string CodigoCheckIn { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public int? PagoID { get; set; }
    }

    public class PerfilInscripcionesDTO
    {
        public List<InscripcionEventoVista> MisEventos { get; set; } = new List<InscripcionEventoVista>();
        public List<InscripcionCompetenciaVista> MisCompetencias { get; set; } = new List<InscripcionCompetenciaVista>();
    }
}
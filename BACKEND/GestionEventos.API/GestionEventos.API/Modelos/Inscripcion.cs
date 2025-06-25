namespace GestionEventos.API.Modelos
{
    public class Inscripcion
    {
        public int InscripcionID { get; set; }
        public int UsuarioID { get; set; }
        public string TipoActividad { get; set; } = string.Empty; 
        public int ActividadID { get; set; }
        public int? TipoEntradaID { get; set; } 
        public int? EquipoID { get; set; } 
        public string CodigoCheckIn { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty; 
        public DateTime FechaInscripcion { get; set; }
        public DateTime? FechaCheckIn { get; set; }
    }
}
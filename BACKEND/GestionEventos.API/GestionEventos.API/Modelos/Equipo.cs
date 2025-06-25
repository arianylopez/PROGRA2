namespace GestionEventos.API.Modelos
{
    public class Equipo
    {
        public int EquipoID { get; set; }
        public int CompetenciaID { get; set; }
        public string NombreEquipo { get; set; } = string.Empty;
        public int LiderUsuarioID { get; set; }
        public DateTime FechaCreacion { get; set; }
        public string Estado { get; set; } = string.Empty; 
    }
}
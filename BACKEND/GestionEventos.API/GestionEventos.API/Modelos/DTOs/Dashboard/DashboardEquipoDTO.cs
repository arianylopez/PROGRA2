namespace GestionEventos.API.Modelos.DTOs.Dashboard
{
    public class DashboardEquipoDTO
    {
        public int EquipoID { get; set; }
        public string NombreEquipo { get; set; } = string.Empty;
        public string NombreLider { get; set; } = string.Empty;
        public List<DashboardParticipanteDTO> Miembros { get; set; } = new List<DashboardParticipanteDTO>();
    }
}
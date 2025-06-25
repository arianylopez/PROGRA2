namespace GestionEventos.API.Modelos.DTOs.Dashboard
{
    public class DashboardParticipanteDTO
    {
        public string NombreParticipante { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string TipoEntrada { get; set; } = string.Empty;
        public string CodigoCheckIn { get; set; } = string.Empty;
        public DateTime? FechaCheckIn { get; set; }
    }
}
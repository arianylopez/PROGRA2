namespace GestionEventos.API.Modelos.DTOs.Dashboard
{
    public class DashboardEventoDTO
    {
        public string TituloEvento { get; set; } = string.Empty;
        public int TotalInscritos { get; set; }
        public int TotalAsistentes { get; set; } 
        public decimal TotalRecaudado { get; set; }
        public List<DashboardRenglonVentaDTO> DetalleVentas { get; set; } = new List<DashboardRenglonVentaDTO>();
        public List<DashboardParticipanteDTO> Participantes { get; set; } = new List<DashboardParticipanteDTO>();
    }
}
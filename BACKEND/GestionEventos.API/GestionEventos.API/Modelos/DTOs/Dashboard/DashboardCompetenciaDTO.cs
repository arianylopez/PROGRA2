using System.Collections.Generic;

namespace GestionEventos.API.Modelos.DTOs.Dashboard
{
    public class DashboardCompetenciaDTO
    {
        public string TituloCompetencia { get; set; } = string.Empty;
        public string TipoCompetencia { get; set; } = string.Empty; 
        public string ModalidadPago { get; set; } = string.Empty; 
        public int TotalInscritos { get; set; }
        public int TotalAsistentes { get; set; }
        public decimal TotalRecaudado { get; set; }
        public List<DashboardRenglonVentaDTO> DetalleVentas { get; set; } = new List<DashboardRenglonVentaDTO>();

        public List<DashboardParticipanteDTO> ParticipantesIndividuales { get; set; } = new List<DashboardParticipanteDTO>();
        public List<DashboardEquipoDTO> Equipos { get; set; } = new List<DashboardEquipoDTO>();
    }
}
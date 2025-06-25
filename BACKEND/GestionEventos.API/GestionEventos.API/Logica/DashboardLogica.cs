using GestionEventos.API.Datos;
using GestionEventos.API.Modelos.DTOs.Dashboard;

namespace GestionEventos.API.Logica
{
    public class DashboardLogica
    {
        private readonly DashboardDAO _dashboardDAO;

        public DashboardLogica(IConfiguration configuration)
        {
            var cadena = configuration.GetConnectionString("SqlConnection") ?? "";
            _dashboardDAO = new DashboardDAO(cadena);
        }

        public DashboardEventoDTO? GetDashboardEvento(int eventoId, int organizadorId)
        {
            return _dashboardDAO.GetDashboardEvento(eventoId, organizadorId);
        }

        public DashboardCompetenciaDTO? GetDashboardCompetencia(int competenciaId, int organizadorId)
        {
            return _dashboardDAO.GetDashboardCompetencia(competenciaId, organizadorId);
        }
    }
}
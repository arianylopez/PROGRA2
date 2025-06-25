using GestionEventos.API.Datos;
using GestionEventos.API.Modelos.DTOs;

namespace GestionEventos.API.Logica
{
    public class CheckInLogica
    {
        private readonly InscripcionDAO _inscripcionDAO;
        private readonly EventoDAO _eventoDAO;
        private readonly CompetenciaDAO _competenciaDAO; 

        public CheckInLogica(IConfiguration configuration)
        {
            var cadena = configuration.GetConnectionString("SqlConnection") ?? "";
            _inscripcionDAO = new InscripcionDAO(cadena);
            _eventoDAO = new EventoDAO(cadena);
            _competenciaDAO = new CompetenciaDAO(cadena);
        }

        public (bool Exito, string Mensaje) ValidarCodigo(CheckInRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.CodigoCheckIn))
            {
                return (false, "El código no puede estar vacío.");
            }

            bool esPropietario = false;
            if (request.TipoActividad.ToLower() == "evento")
            {
                var evento = _eventoDAO.ObtenerEventoPorId(request.ActividadID);
                if (evento != null && evento.OrganizadorID == request.OrganizadorID) esPropietario = true;
            }
            else if (request.TipoActividad.ToLower() == "competencia")
            {
                var competencia = _competenciaDAO.ObtenerCompetenciaPorId(request.ActividadID);
                if (competencia != null && competencia.OrganizadorID == request.OrganizadorID) esPropietario = true;
            }

            if (!esPropietario)
            {
                return (false, "No autorizado para realizar esta acción.");
            }

            return _inscripcionDAO.RegistrarCheckIn(request.TipoActividad, request.ActividadID, request.CodigoCheckIn);
        }
    }
}
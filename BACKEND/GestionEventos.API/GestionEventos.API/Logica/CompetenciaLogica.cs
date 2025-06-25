using GestionEventos.API.Datos;
using GestionEventos.API.Modelos;
using GestionEventos.API.Modelos.DTOs;

namespace GestionEventos.API.Logica
{
    public class CompetenciaLogica
    {
        private readonly CompetenciaDAO _competenciaDAO;
        private readonly InscripcionDAO _inscripcionDAO;
        private readonly string _cadenaSQL;

        public CompetenciaLogica(IConfiguration configuration)
        {
            _cadenaSQL = configuration.GetConnectionString("SqlConnection") ?? "";
            _competenciaDAO = new CompetenciaDAO(_cadenaSQL);
            _inscripcionDAO = new InscripcionDAO(_cadenaSQL);
        }

        public (bool Exito, string Mensaje, Competencia? Competencia) CrearCompetencia(CrearCompetenciaRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Titulo) || request.FechaFin <= request.FechaInicio)
                return (false, "El título es obligatorio y la fecha de fin debe ser posterior a la de inicio.", null);

            if (request.Tipo == "Por Equipos")
            {
                if (!request.TamanoMinEquipo.HasValue || request.TamanoMinEquipo.Value <= 0)
                {
                    return (false, "Para competencias por equipos, debe especificar un tamaño mínimo de equipo mayor a cero.", null);
                }
                if (!request.TamanoMaxEquipo.HasValue || request.TamanoMaxEquipo.Value < request.TamanoMinEquipo.Value)
                {
                    return (false, "El tamaño máximo del equipo debe ser igual o mayor al mínimo.", null);

                }
            }

            if (request.Entradas.Any(e => e.Precio < 0))
            {
                return (false, "El precio de las inscripciones no puede ser negativo.", null);
            }

            if (request.ModalidadPago == "DePago" && (!request.Entradas.Any() || request.Entradas.Any(e => string.IsNullOrWhiteSpace(e.Nombre))))
            {
                return (false, "Para una competencia de pago, debe existir al menos un tipo de inscripción con nombre.", null);

            }

            if (request.Tipo == "Individual")
            {
                request.TamanoMinEquipo = null;
                request.TamanoMaxEquipo = null;
            }

            try
            {
                var competenciaCreada = _competenciaDAO.CrearCompetencia(request);
                return (true, "Competencia creada con éxito.", competenciaCreada);
            }
            catch (Exception ex)
            {
                return (false, ex.Message, null);
            }
        }

        public bool ActualizarCompetencia(ActualizarCompetenciaRequest request)
        {
            var competenciaOriginal = _competenciaDAO.ObtenerCompetenciaPorId(request.CompetenciaID);
            if (competenciaOriginal == null || competenciaOriginal.OrganizadorID != request.UsuarioIDQueActualiza)
            {
                return false;
            }
            return _competenciaDAO.ActualizarCompetencia(request);
        }

        public (bool Exito, string Mensaje) EliminarCompetencia(int id, int usuarioId)
        {
            var competencia = _competenciaDAO.ObtenerCompetenciaPorId(id);
            if (competencia == null)
            {
                return (false, "La competencia no existe.");
            }
            if (competencia.OrganizadorID != usuarioId)
            {
                return (false, "No tienes permiso para eliminar esta competencia.");
            }

            bool tieneInscritos = _inscripcionDAO.VerificarInscripcionExistente(0, "Competencia", id);

            if (tieneInscritos)
            {
                var exito = _competenciaDAO.DeshabilitarCompetenciaYReembolsar(id);
                return exito ? (true, "La competencia tenía inscritos y ha sido deshabilitada. Se simuló el reembolso.") : (false, "Error al deshabilitar la competencia.");
            }
            else
            {
                var exito = _competenciaDAO.EliminarCompetenciaFisicamente(id);
                return exito ? (true, "Competencia eliminada permanentemente.") : (false, "Error al eliminar la competencia.");
            }
        }

        public CompetenciaDetalleDTO? ObtenerCompetenciaPorId(int id)
        {
            return _competenciaDAO.ObtenerCompetenciaPorId(id);
        }

        public List<CompetenciaPublicaDTO> ListarCompetenciasPublicas(int? categoriaId = null)
        {
            return _competenciaDAO.ListarCompetenciasPublicas(categoriaId);
        }

        public List<Competencia> ListarCompetenciasPorOrganizador(int organizadorId)
        {
            return _competenciaDAO.ListarCompetenciasPorOrganizador(organizadorId);
        }
    }
}
using GestionEventos.API.Datos;
using GestionEventos.API.Modelos;
using GestionEventos.API.Modelos.DTOs;

namespace GestionEventos.API.Logica
{
    public class EventoLogica
    {
        private readonly EventoDAO _eventoDAO;
        private readonly InscripcionDAO _inscripcionDAO; 
        private readonly string _cadenaSQL;

        public EventoLogica(IConfiguration configuration)
        {
            _cadenaSQL = configuration.GetConnectionString("SqlConnection") ?? "";
            _eventoDAO = new EventoDAO(_cadenaSQL);
            _inscripcionDAO = new InscripcionDAO(_cadenaSQL); 
        }

        public (bool Exito, string Mensaje, Evento? Evento) CrearEvento(CrearEventoRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Titulo) || request.FechaEvento <= DateTime.Now)
                return (false, "El título es obligatorio y la fecha debe ser en el futuro.", null);

            if (request.Entradas.Any(e => e.Precio < 0))
                return (false, "El precio de las entradas no puede ser negativo.", null);

            if (!request.Entradas.Any() || request.Entradas.Any(e => string.IsNullOrWhiteSpace(e.Nombre)))
                return (false, "Debe existir al menos un tipo de entrada con nombre.", null);

            try
            {
                var eventoCreado = _eventoDAO.CrearEvento(request);
                return (true, "Evento creado con éxito.", eventoCreado);
            }
            catch (Exception ex)
            {
                return (false, ex.Message, null);
            }
        }


        public List<EventoPublicoDTO> ListarEventosDisponibles()
        {
            return _eventoDAO.ListarEventosDisponibles();
        }

        public EventoDetalleDTO? ObtenerEventoPorId(int id)
        {
            return _eventoDAO.ObtenerEventoPorId(id);
        }


        public bool ActualizarEvento(ActualizarEventoRequest request)
        {
            var eventoOriginal = _eventoDAO.ObtenerEventoPorId(request.EventoID);
            if (eventoOriginal == null || eventoOriginal.OrganizadorID != request.UsuarioIDQueActualiza)
            {
                return false;
            }
            return _eventoDAO.ActualizarEvento(request);
        }

        public (bool Exito, string Mensaje) EliminarEvento(int id, int usuarioId)
        {
            var evento = _eventoDAO.ObtenerEventoPorId(id);
            if (evento == null)
            {
                return (false, "El evento no existe.");
            }
            if (evento.OrganizadorID != usuarioId)
            {
                return (false, "No tienes permiso para eliminar este evento.");
            }

            bool tieneInscritos = _inscripcionDAO.VerificarInscripcionExistente(0, "Evento", id);

            if (tieneInscritos)
            {
                var exito = _eventoDAO.DeshabilitarEventoYReembolsar(id);
                return exito ? (true, "El evento tenía inscripciones y ha sido deshabilitado. Se ha simulado el reembolso.") : (false, "Error al deshabilitar el evento.");
            }
            else
            {
                var exito = _eventoDAO.EliminarEventoFisicamente(id);
                return exito ? (true, "Evento eliminado permanentemente.") : (false, "Error al eliminar el evento.");
            }
        }

        public List<Evento> ListarEventosPorOrganizador(int organizadorId)
        {
            return _eventoDAO.ListarEventosPorOrganizador(organizadorId);
        }

        public List<EventoPublicoDTO> ListarEventosPublicos(int? categoriaId = null)
        {
            return _eventoDAO.ListarEventosPublicos(categoriaId);
        }
    }
}
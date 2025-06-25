using GestionEventos.API.Datos;
using GestionEventos.API.Modelos;
using System.Threading.Tasks;

namespace GestionEventos.API.Logica
{
    public class ResenaLogica
    {
        private readonly ResenaDAO _resenaDAO;
        public ResenaLogica(IConfiguration configuration)
        {
            _resenaDAO = new ResenaDAO(configuration);
        }

        public async Task<List<Resena>> ObtenerResenasPorEvento(int eventoId)
        {
            return await _resenaDAO.ObtenerResenasPorEvento(eventoId);
        }

        public async Task<(bool Exito, string Mensaje)> CrearResena(Resena nuevaResena)
        {
            if (nuevaResena == null)
                return (false, "La reseña no puede ser nula.");

            bool tieneCalificacionValida = nuevaResena.Calificacion.HasValue && nuevaResena.Calificacion.Value > 0;
            bool tieneComentarioValido = !string.IsNullOrWhiteSpace(nuevaResena.Comentario);

            if (!tieneCalificacionValida && !tieneComentarioValido)
            {
                return (false, "Debes proporcionar al menos una calificación o un comentario.");
            }

            if (nuevaResena.Calificacion.HasValue && (nuevaResena.Calificacion.Value < 1 || nuevaResena.Calificacion.Value > 5))
            {
                return (false, "La calificación debe estar entre 1 y 5.");
            }

            nuevaResena.Fecha = DateTime.UtcNow;
            await _resenaDAO.CrearResena(nuevaResena);
            return (true, "Reseña creada con éxito.");
        }
    }
}
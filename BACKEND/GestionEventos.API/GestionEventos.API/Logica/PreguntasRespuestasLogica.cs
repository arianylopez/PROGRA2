using GestionEventos.API.Datos;
using GestionEventos.API.Modelos;

namespace GestionEventos.API.Logica
{
    public class PreguntasRespuestasLogica
    {
        private readonly PreguntasRespuestasDAO _qnaDAO;
        private readonly EventoDAO _eventoDAO;
        private readonly CompetenciaDAO _competenciaDAO;

        public PreguntasRespuestasLogica(IConfiguration config)
        {
            var cadena = config.GetConnectionString("SqlConnection") ?? "";
            _qnaDAO = new PreguntasRespuestasDAO(config);
            _eventoDAO = new EventoDAO(cadena);
            _competenciaDAO = new CompetenciaDAO(cadena);
        }

        public async Task<bool> ResponderPregunta(string preguntaId, Respuesta respuesta)
        {
            var preguntaDoc = await _qnaDAO.GetById(preguntaId);
            if (preguntaDoc == null) return false;

            int organizadorId = 0;
            if (preguntaDoc.TipoActividad == "evento")
            {
                var evento = _eventoDAO.ObtenerEventoPorId(preguntaDoc.ActividadId);
                if (evento != null) organizadorId = evento.OrganizadorID;
            }
            else
            {
                var competencia = _competenciaDAO.ObtenerCompetenciaPorId(preguntaDoc.ActividadId);
                if (competencia != null) organizadorId = competencia.OrganizadorID;
            }

            if (organizadorId != 0 && organizadorId == respuesta.UsuarioId)
            {
                respuesta.Fecha = DateTime.UtcNow;
                return await _qnaDAO.ResponderPregunta(preguntaId, respuesta);
            }
            return false;
        }

        public async Task<List<QnADocument>> GetByActividadId(string tipo, int id) => await _qnaDAO.GetByActividadId(tipo, id);

        public async Task HacerPregunta(QnADocument doc)
        {
            doc.Pregunta.Fecha = DateTime.UtcNow;
            await _qnaDAO.HacerPregunta(doc);
        }
    }
}
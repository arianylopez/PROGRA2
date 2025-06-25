using GestionEventos.API.Modelos;
using MongoDB.Bson;
using MongoDB.Driver;

namespace GestionEventos.API.Datos
{
    public class PreguntasRespuestasDAO
    {
        private readonly MongoDbContext _context;
        public PreguntasRespuestasDAO(IConfiguration config) { _context = new MongoDbContext(config); }

        public async Task<List<QnADocument>> GetByActividadId(string tipoActividad, int actividadId)
        {
            return await _context.PreguntasYRespuestas
                .Find(q => q.TipoActividad == tipoActividad && q.ActividadId == actividadId)
                .ToListAsync();
        }

        public async Task<QnADocument?> GetById(string id)
        {
            return await _context.PreguntasYRespuestas.Find(q => q.Id == id).FirstOrDefaultAsync();
        }

        public async Task HacerPregunta(QnADocument pregunta)
        {
            await _context.PreguntasYRespuestas.InsertOneAsync(pregunta);
        }

        public async Task<bool> ResponderPregunta(string id, Respuesta respuesta)
        {
            var filter = Builders<QnADocument>.Filter.Eq(q => q.Id, id);
            var update = Builders<QnADocument>.Update.Set(q => q.Respuesta, respuesta);
            var result = await _context.PreguntasYRespuestas.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }
    }
}
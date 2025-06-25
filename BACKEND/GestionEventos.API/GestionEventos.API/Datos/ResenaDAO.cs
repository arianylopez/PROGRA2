using GestionEventos.API.Modelos;
using MongoDB.Driver;

namespace GestionEventos.API.Datos
{
    public class ResenaDAO
    {
        private readonly MongoDbContext _context;

        public ResenaDAO(IConfiguration configuration)
        {
            _context = new MongoDbContext(configuration);
        }

        public async Task<List<Resena>> ObtenerResenasPorEvento(int eventoId)
        {
            return await _context.Resenas.Find(r => r.EventoId == eventoId).ToListAsync();
        }

        public async Task CrearResena(Resena nuevaResena)
        {
            await _context.Resenas.InsertOneAsync(nuevaResena);
        }
    }
}
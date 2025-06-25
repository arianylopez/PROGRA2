using GestionEventos.API.Modelos;
using MongoDB.Driver;

namespace GestionEventos.API.Datos
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;

        public MongoDbContext(IConfiguration configuration)
        {
            var connectionString = configuration.GetValue<string>("MongoDbSettings:ConnectionString");
            var databaseName = configuration.GetValue<string>("MongoDbSettings:DatabaseName");

            var client = new MongoClient(connectionString);
            _database = client.GetDatabase(databaseName);
        }

        public IMongoCollection<Resena> Resenas => _database.GetCollection<Resena>("reseñas");
        public IMongoCollection<QnADocument> PreguntasYRespuestas => _database.GetCollection<QnADocument>("preguntas_y_respuestas");
    }
}
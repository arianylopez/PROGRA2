using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GestionEventos.API.Modelos
{
    public class Resena
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        [BsonElement("eventoId")]
        public int EventoId { get; set; }

        [BsonElement("usuarioId")]
        public int UsuarioId { get; set; }

        [BsonElement("nombreUsuario")]
        public string NombreUsuario { get; set; } = string.Empty;

        [BsonElement("calificacion")]
        [BsonIgnoreIfNull] 
        public int? Calificacion { get; set; }

        [BsonElement("comentario")]
        [BsonIgnoreIfNull] 
        public string? Comentario { get; set; }

        [BsonElement("fecha")]
        public DateTime Fecha { get; set; }
    }
}
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GestionEventos.API.Modelos
{
    public class QnADocument
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        [BsonElement("tipoActividad")]
        public string TipoActividad { get; set; } = string.Empty; 

        [BsonElement("actividadId")]
        public int ActividadId { get; set; }

        [BsonElement("pregunta")]
        public Pregunta Pregunta { get; set; } = new Pregunta();

        [BsonElement("respuesta")]
        [BsonIgnoreIfNull] 
        public Respuesta? Respuesta { get; set; }
    }
}
using GestionEventos.API.Modelos;

namespace GestionEventos.API.Modelos.DTOs
{
    public class ActualizarEventoRequest
    {
        public int EventoID { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public DateTime FechaEvento { get; set; }
        public string Ubicacion { get; set; } = string.Empty;
        public string? ImagenUrl { get; set; }
        public int UsuarioIDQueActualiza { get; set; }
        public List<int> CategoriaIDs { get; set; } = new List<int>();
    }
}
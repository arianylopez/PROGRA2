using GestionEventos.API.Modelos;

namespace GestionEventos.API.Modelos.DTOs
{
    public class CrearEventoRequest
    {
        public string Titulo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public DateTime FechaEvento { get; set; }
        public string Ubicacion { get; set; } = string.Empty;
        public int OrganizadorID { get; set; }
        public string? ImagenUrl { get; set; }
        public List<TiposEntrada> Entradas { get; set; } = new List<TiposEntrada>();
        public List<int> CategoriaIDs { get; set; } = new List<int>();
    }
}
using GestionEventos.API.Modelos;

namespace GestionEventos.API.Modelos.DTOs
{
    public class CrearCompetenciaRequest
    {
        public string Titulo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public int? TamanoMinEquipo { get; set; }
        public int? TamanoMaxEquipo { get; set; }
        public int OrganizadorID { get; set; }
        public string? ImagenUrl { get; set; }
        public string ModalidadPago { get; set; } = string.Empty;
        public List<int> CategoriaIDs { get; set; } = new List<int>();
        public List<TiposEntradaCompetencia> Entradas { get; set; } = new List<TiposEntradaCompetencia>();
    }
}
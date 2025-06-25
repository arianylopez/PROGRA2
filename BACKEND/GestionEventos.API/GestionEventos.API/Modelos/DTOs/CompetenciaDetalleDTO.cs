using GestionEventos.API.Modelos;

namespace GestionEventos.API.Modelos.DTOs
{
    public class CompetenciaDetalleDTO : Competencia
    {
        public List<Categoria> Categorias { get; set; } = new List<Categoria>();
        public List<TiposEntradaCompetencia> Entradas { get; set; } = new List<TiposEntradaCompetencia>();
    }
}
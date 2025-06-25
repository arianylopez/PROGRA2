using GestionEventos.API.Modelos;

namespace GestionEventos.API.Modelos.DTOs
{
    public class EventoDetalleDTO : Evento // HERENCIA
    {
        public List<TiposEntrada> Entradas { get; set; } = new List<TiposEntrada>();
        public List<Categoria> Categorias { get; set; } = new List<Categoria>();
    }
}
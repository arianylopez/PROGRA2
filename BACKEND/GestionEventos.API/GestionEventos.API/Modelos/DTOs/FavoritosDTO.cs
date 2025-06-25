namespace GestionEventos.API.Modelos.DTOs
{
    public class FavoritosDTO
    {
        public List<EventoPublicoDTO> Eventos { get; set; } = new List<EventoPublicoDTO>();
        public List<CompetenciaPublicaDTO> Competencias { get; set; } = new List<CompetenciaPublicaDTO>();
    }
}
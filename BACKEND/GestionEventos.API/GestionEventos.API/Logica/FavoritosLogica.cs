using GestionEventos.API.Datos;
using GestionEventos.API.Modelos.DTOs;

namespace GestionEventos.API.Logica
{
    public class FavoritosLogica
    {
        private readonly FavoritosDAO _favoritosDAO;
        public FavoritosLogica(IConfiguration configuration)
        {
            var cadena = configuration.GetConnectionString("SqlConnection") ?? "";
            _favoritosDAO = new FavoritosDAO(cadena);
        }

        public bool AgregarFavoritoEvento(int usuarioId, int eventoId) => _favoritosDAO.AgregarFavoritoEvento(usuarioId, eventoId);
        public bool EliminarFavoritoEvento(int usuarioId, int eventoId) => _favoritosDAO.EliminarFavoritoEvento(usuarioId, eventoId);

        public bool AgregarFavoritoCompetencia(int usuarioId, int competenciaId) => _favoritosDAO.AgregarFavoritoCompetencia(usuarioId, competenciaId);
        public bool EliminarFavoritoCompetencia(int usuarioId, int competenciaId) => _favoritosDAO.EliminarFavoritoCompetencia(usuarioId, competenciaId);

        public FavoritosDTO ListarFavoritosPorUsuario(int usuarioId) => _favoritosDAO.ListarFavoritosPorUsuario(usuarioId);
    }
}
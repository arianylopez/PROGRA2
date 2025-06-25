using GestionEventos.API.Datos;
using GestionEventos.API.Modelos;

namespace GestionEventos.API.Logica
{
    public class CategoriaLogica
    {
        private readonly CategoriaDAO _categoriaDAO;

        public CategoriaLogica(IConfiguration configuration)
        {
            var cadena = configuration.GetConnectionString("SqlConnection") ?? "";
            _categoriaDAO = new CategoriaDAO(cadena);
        }

        public List<Categoria> ListarCategorias()
        {
            return _categoriaDAO.ListarCategorias();
        }
    }
}
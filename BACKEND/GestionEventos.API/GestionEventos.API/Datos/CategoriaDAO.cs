using GestionEventos.API.Modelos;
using Microsoft.Data.SqlClient;

namespace GestionEventos.API.Datos
{
    public class CategoriaDAO
    {
        private readonly string _cadenaSQL;
        public CategoriaDAO(string cadenaSQL) 
        { 
            _cadenaSQL = cadenaSQL; 
        }

        public List<Categoria> ListarCategorias()
        {
            var lista = new List<Categoria>();
            using (var conexion = new SqlConnection(_cadenaSQL))
            {
                conexion.Open();
                var cmd = new SqlCommand("SELECT * FROM Categorias ORDER BY Nombre", conexion);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        lista.Add(new Categoria
                        {
                            CategoriaID = Convert.ToInt32(reader["CategoriaID"]),
                            Nombre = reader["Nombre"].ToString() ?? ""
                        });
                    }
                }
            }
            return lista;
        }
    }
}
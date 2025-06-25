using GestionEventos.API.Modelos.DTOs;
using Microsoft.Data.SqlClient;

namespace GestionEventos.API.Datos
{
    public class FavoritosDAO
    {
        private readonly string _cadenaSQL;
        public FavoritosDAO(string cadenaSQL) { _cadenaSQL = cadenaSQL; }

        public bool AgregarFavoritoEvento(int usuarioId, int eventoId)
        {
            using (var conexion = new SqlConnection(_cadenaSQL))
            {
                conexion.Open();
                var sql = "INSERT INTO FavoritosEventos (UsuarioID, EventoID) VALUES (@usuarioId, @eventoId)";
                var cmd = new SqlCommand(sql, conexion);
                cmd.Parameters.AddWithValue("@usuarioId", usuarioId);
                cmd.Parameters.AddWithValue("@eventoId", eventoId);
                try { return cmd.ExecuteNonQuery() > 0; }
                catch (SqlException) { return false; }
            }
        }

        public bool EliminarFavoritoEvento(int usuarioId, int eventoId)
        {
            using (var conexion = new SqlConnection(_cadenaSQL))
            {
                conexion.Open();
                var sql = "DELETE FROM FavoritosEventos WHERE UsuarioID = @usuarioId AND EventoID = @eventoId";
                var cmd = new SqlCommand(sql, conexion);
                cmd.Parameters.AddWithValue("@usuarioId", usuarioId);
                cmd.Parameters.AddWithValue("@eventoId", eventoId);
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        public bool AgregarFavoritoCompetencia(int usuarioId, int competenciaId)
        {
            using (var conexion = new SqlConnection(_cadenaSQL))
            {
                conexion.Open();
                var sql = "INSERT INTO FavoritosCompetencias (UsuarioID, CompetenciaID) VALUES (@usuarioId, @competenciaId)";
                var cmd = new SqlCommand(sql, conexion);
                cmd.Parameters.AddWithValue("@usuarioId", usuarioId);
                cmd.Parameters.AddWithValue("@competenciaId", competenciaId);
                try { return cmd.ExecuteNonQuery() > 0; }
                catch (SqlException) { return false; }
            }
        }

        public bool EliminarFavoritoCompetencia(int usuarioId, int competenciaId)
        {
            using (var conexion = new SqlConnection(_cadenaSQL))
            {
                conexion.Open();
                var sql = "DELETE FROM FavoritosCompetencias WHERE UsuarioID = @usuarioId AND CompetenciaID = @competenciaId";
                var cmd = new SqlCommand(sql, conexion);
                cmd.Parameters.AddWithValue("@usuarioId", usuarioId);
                cmd.Parameters.AddWithValue("@competenciaId", competenciaId);
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        public FavoritosDTO ListarFavoritosPorUsuario(int usuarioId)
        {
            var favoritos = new FavoritosDTO();
            using (var conexion = new SqlConnection(_cadenaSQL))
            {
                conexion.Open();

                var sqlEventos = @"SELECT e.EventoID, e.Titulo, e.FechaEvento, e.Ubicacion, u.Nombre as OrganizadorNombre 
                                   FROM Eventos e 
                                   JOIN FavoritosEventos f ON e.EventoID = f.EventoID 
                                   JOIN Usuarios u ON e.OrganizadorID = u.UsuarioID
                                   WHERE f.UsuarioID = @usuarioId";
                var cmdEventos = new SqlCommand(sqlEventos, conexion);
                cmdEventos.Parameters.AddWithValue("@usuarioId", usuarioId);
                using (var reader = cmdEventos.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        favoritos.Eventos.Add(new EventoPublicoDTO
                        {
                            EventoID = Convert.ToInt32(reader["EventoID"]),
                            Titulo = reader["Titulo"].ToString() ?? "",
                            FechaEvento = Convert.ToDateTime(reader["FechaEvento"]),
                            Ubicacion = reader["Ubicacion"].ToString() ?? "",
                            OrganizadorNombre = reader["OrganizadorNombre"].ToString() ?? ""
                        });
                    }
                }

                var sqlCompetencias = @"SELECT c.CompetenciaID, c.Titulo, c.FechaFin, c.Tipo, u.Nombre as OrganizadorNombre 
                                        FROM Competencias c
                                        JOIN FavoritosCompetencias f ON c.CompetenciaID = f.CompetenciaID
                                        JOIN Usuarios u ON c.OrganizadorID = u.UsuarioID
                                        WHERE f.UsuarioID = @usuarioId";
                var cmdCompetencias = new SqlCommand(sqlCompetencias, conexion);
                cmdCompetencias.Parameters.AddWithValue("@usuarioId", usuarioId);
                using (var reader = cmdCompetencias.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        favoritos.Competencias.Add(new CompetenciaPublicaDTO
                        {
                            CompetenciaID = Convert.ToInt32(reader["CompetenciaID"]),
                            Titulo = reader["Titulo"].ToString() ?? "",
                            FechaFin = Convert.ToDateTime(reader["FechaFin"]),
                            Tipo = reader["Tipo"].ToString() ?? "",
                            OrganizadorNombre = reader["OrganizadorNombre"].ToString() ?? ""
                        });
                    }
                }
            }
            return favoritos;
        }
    }
}
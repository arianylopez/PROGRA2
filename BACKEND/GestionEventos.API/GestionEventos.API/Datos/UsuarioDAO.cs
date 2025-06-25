using GestionEventos.API.Modelos;
using Microsoft.Data.SqlClient;
using System.Data;

namespace GestionEventos.API.Datos
{
    public class UsuarioDAO
    {
        private readonly string _cadenaSQL;
        public UsuarioDAO(string cadenaSQL) { _cadenaSQL = cadenaSQL; }

        public List<PreguntaSeguridad> ListarPreguntasSeguridad()
        {
            var lista = new List<PreguntaSeguridad>();
            using (var conexion = new SqlConnection(_cadenaSQL))
            {
                conexion.Open();
                var comando = new SqlCommand("SELECT PreguntaID, TextoPregunta FROM PreguntasSeguridad", conexion);
                using (var reader = comando.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        lista.Add(new PreguntaSeguridad
                        {
                            PreguntaID = Convert.ToInt32(reader["PreguntaID"]),
                            TextoPregunta = reader["TextoPregunta"].ToString() ?? ""
                        });
                    }
                }
            }
            return lista;
        }

        public Usuario? ObtenerPorEmail(string email)
        {
            Usuario? usuario = null;
            using (var conexion = new SqlConnection(_cadenaSQL))
            {
                conexion.Open();
                var comando = new SqlCommand("SELECT * FROM Usuarios WHERE Email = @email", conexion);
                comando.Parameters.AddWithValue("@email", email);
                using (var reader = comando.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        usuario = new Usuario
                        {
                            UsuarioID = Convert.ToInt32(reader["UsuarioID"]),
                            Nombre = reader["Nombre"].ToString() ?? "",
                            Email = reader["Email"].ToString() ?? "",
                            Password = reader["Password"].ToString() ?? "",
                            PreguntaID = Convert.ToInt32(reader["PreguntaID"]),
                            RespuestaSeguridad = reader["RespuestaSeguridad"].ToString() ?? ""
                        };
                    }
                }
            }
            return usuario;
        }

        public Usuario? ObtenerUsuarioPorId(int id)
        {
            Usuario? usuario = null;
            using (var conexion = new SqlConnection(_cadenaSQL))
            {
                conexion.Open();
                var comando = new SqlCommand("SELECT * FROM Usuarios WHERE UsuarioID = @id", conexion);
                comando.Parameters.AddWithValue("@id", id);
                using (var reader = comando.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        usuario = new Usuario
                        {
                            UsuarioID = Convert.ToInt32(reader["UsuarioID"]),
                            Nombre = reader["Nombre"].ToString() ?? "",
                            Email = reader["Email"].ToString() ?? "",
                            Password = reader["Password"].ToString() ?? "",
                            PreguntaID = Convert.ToInt32(reader["PreguntaID"]),
                            RespuestaSeguridad = reader["RespuestaSeguridad"].ToString() ?? ""
                        };
                    }
                }
            }
            return usuario;
        }

        public List<Usuario> ObtenerUsuariosPorEmails(List<string> emails)
        {
            var usuarios = new List<Usuario>();
            if (emails == null || !emails.Any())
            {
                return usuarios;
            }
            using (var conexion = new SqlConnection(_cadenaSQL))
            {
                conexion.Open();
                var parametros = new List<string>();
                var cmd = new SqlCommand();
                for (int i = 0; i < emails.Count; i++)
                {
                    var paramName = $"@email{i}";
                    parametros.Add(paramName);
                    cmd.Parameters.AddWithValue(paramName, emails[i]);
                }
                cmd.Connection = conexion;
                cmd.CommandText = $"SELECT UsuarioID, Nombre, Email FROM Usuarios WHERE Email IN ({string.Join(", ", parametros)})";
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        usuarios.Add(new Usuario
                        {
                            UsuarioID = Convert.ToInt32(reader["UsuarioID"]),
                            Nombre = reader["Nombre"].ToString() ?? "",
                            Email = reader["Email"].ToString() ?? ""
                        });
                    }
                }
            }
            return usuarios;
        }

        public (bool Exito, string Mensaje, Usuario? Usuario) RegistrarUsuario(Usuario nuevoUsuario)
        {
            using (var conexion = new SqlConnection(_cadenaSQL))
            {
                conexion.Open();
                var cmd = new SqlCommand("sp_RegistrarUsuario", conexion)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@Nombre", nuevoUsuario.Nombre);
                cmd.Parameters.AddWithValue("@Email", nuevoUsuario.Email);
                cmd.Parameters.AddWithValue("@Password", nuevoUsuario.Password);
                cmd.Parameters.AddWithValue("@PreguntaID", nuevoUsuario.PreguntaID);
                cmd.Parameters.AddWithValue("@RespuestaSeguridad", nuevoUsuario.RespuestaSeguridad);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        bool exito = Convert.ToInt32(reader["Exito"]) == 1;
                        string mensaje = reader["Mensaje"].ToString() ?? "";

                        if (exito)
                        {
                            nuevoUsuario.UsuarioID = Convert.ToInt32(reader["NuevoUsuarioID"]);
                            return (true, mensaje, nuevoUsuario);
                        }
                        else
                        {
                            return (false, mensaje, null);
                        }
                    }
                }
            }
            return (false, "Error inesperado al contactar la base de datos.", null);
        }

        public bool ActualizarPassword(int usuarioId, string nuevaPassword)
        {
            using (var conexion = new SqlConnection(_cadenaSQL))
            {
                conexion.Open();
                var sql = "UPDATE Usuarios SET Password = @password WHERE UsuarioID = @id";
                var cmd = new SqlCommand(sql, conexion);
                cmd.Parameters.AddWithValue("@password", nuevaPassword);
                cmd.Parameters.AddWithValue("@id", usuarioId);
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        public bool ActualizarPerfil(int usuarioId, string nombre, string email)
        {
            using (var conexion = new SqlConnection(_cadenaSQL))
            {
                conexion.Open();
                var sql = "UPDATE Usuarios SET Nombre = @nombre, Email = @email WHERE UsuarioID = @id AND NOT EXISTS (SELECT 1 FROM Usuarios WHERE Email = @email AND UsuarioID != @id)";
                var cmd = new SqlCommand(sql, conexion);
                cmd.Parameters.AddWithValue("@nombre", nombre);
                cmd.Parameters.AddWithValue("@email", email);
                cmd.Parameters.AddWithValue("@id", usuarioId);
                return cmd.ExecuteNonQuery() > 0;
            }
        }
    }
}
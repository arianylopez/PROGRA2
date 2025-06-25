using GestionEventos.API.Modelos;
using GestionEventos.API.Modelos.DTOs;
using Microsoft.Data.SqlClient;
using System.Text;

namespace GestionEventos.API.Datos
{
    public class InscripcionDAO
    {
        private readonly string _cadenaSQL;
        public InscripcionDAO(string cadenaSQL) 
        { 
            _cadenaSQL = cadenaSQL; 
        }

        public List<Inscripcion> CrearInscripciones(List<Inscripcion> inscripciones, SqlConnection conexion, SqlTransaction transaccion)
        {
            foreach (var inscripcion in inscripciones)
            {
                var sql = "INSERT INTO Inscripciones (UsuarioID, TipoActividad, ActividadID, TipoEntradaID, EquipoID, CodigoCheckIn, Estado, FechaInscripcion) " +
                          "VALUES (@usuarioId, @tipoActividad, @actividadId, @tipoEntradaId, @equipoId, @codigoCheckIn, @estado, GETDATE()); SELECT SCOPE_IDENTITY();";

                using (var cmd = new SqlCommand(sql, conexion, transaccion))
                {
                    cmd.Parameters.AddWithValue("@usuarioId", inscripcion.UsuarioID);
                    cmd.Parameters.AddWithValue("@tipoActividad", inscripcion.TipoActividad);
                    cmd.Parameters.AddWithValue("@actividadId", inscripcion.ActividadID);
                    cmd.Parameters.AddWithValue("@codigoCheckIn", Guid.NewGuid().ToString("N").Substring(0, 10).ToUpper()); // generador codigo unico
                    cmd.Parameters.AddWithValue("@estado", inscripcion.Estado);
                    cmd.Parameters.Add(new SqlParameter("@tipoEntradaId", inscripcion.TipoEntradaID.HasValue ? (object)inscripcion.TipoEntradaID.Value : DBNull.Value));
                    cmd.Parameters.Add(new SqlParameter("@equipoId", inscripcion.EquipoID.HasValue ? (object)inscripcion.EquipoID.Value : DBNull.Value));
                    var nuevoId = cmd.ExecuteScalar();
                    inscripcion.InscripcionID = (nuevoId == null || nuevoId == DBNull.Value) ? 0 : Convert.ToInt32(nuevoId);
                    if (inscripcion.InscripcionID == 0) throw new Exception($"No se pudo crear el registro de inscripción para el usuario {inscripcion.UsuarioID}.");
                }
            }
            return inscripciones;
        }

        public bool InscribirGratis(List<Inscripcion> inscripciones)
        {
            using (var conexion = new SqlConnection(_cadenaSQL))
            {
                conexion.Open();
                var transaccion = conexion.BeginTransaction();
                try
                {
                    CrearInscripciones(inscripciones, conexion, transaccion);
                    transaccion.Commit();
                    return true;
                }
                catch (Exception)
                {
                    transaccion.Rollback();
                    return false;
                }
            }
        }

        public void ActualizarEstadoInscripciones(List<int> inscripcionIDs, string nuevoEstado, SqlConnection conexion, SqlTransaction transaccion)
        {
            if (!inscripcionIDs.Any()) return;

            var parametros = new List<string>();
            var cmd = new SqlCommand();
            for (int i = 0; i < inscripcionIDs.Count; i++)
            {
                var paramName = $"@id{i}";
                parametros.Add(paramName);
                cmd.Parameters.AddWithValue(paramName, inscripcionIDs[i]);
            }

            cmd.Connection = conexion;
            cmd.Transaction = transaccion;
            cmd.CommandText = $"UPDATE Inscripciones SET Estado = @estado WHERE InscripcionID IN ({string.Join(", ", parametros)})";
            cmd.Parameters.AddWithValue("@estado", nuevoEstado);
            cmd.ExecuteNonQuery();
        }

        public PerfilInscripcionesDTO ListarMisInscripciones(int usuarioId)
        {
            var resultado = new PerfilInscripcionesDTO();
            using (var conexion = new SqlConnection(_cadenaSQL))
            {
                conexion.Open();
                var cmd = new SqlCommand("SELECT * FROM vw_MisInscripcionesCompletas WHERE UsuarioID = @usuarioId", conexion);
                cmd.Parameters.AddWithValue("@usuarioId", usuarioId);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string tipoActividad = reader["TipoActividad"].ToString() ?? "";
                        if (tipoActividad == "Evento")
                        {
                            resultado.MisEventos.Add(new InscripcionEventoVista
                            {
                                InscripcionID = Convert.ToInt32(reader["InscripcionID"]),
                                TituloEvento = reader["TituloEvento"].ToString() ?? string.Empty,
                                FechaEvento = Convert.ToDateTime(reader["FechaEvento"]),
                                Ubicacion = reader["Ubicacion"].ToString() ?? string.Empty,
                                TipoEntrada = reader["TipoEntrada"].ToString() ?? string.Empty,
                                Precio = reader["PrecioEvento"] != DBNull.Value ? Convert.ToDecimal(reader["PrecioEvento"]) : 0,
                                CodigoCheckIn = reader["CodigoCheckIn"].ToString() ?? string.Empty,
                                Estado = reader["Estado"].ToString() ?? string.Empty,
                                PagoID = reader["PagoID"] as int?
                            });
                        }
                        else if (tipoActividad == "Competencia")
                        {
                            resultado.MisCompetencias.Add(new InscripcionCompetenciaVista
                            {
                                InscripcionID = Convert.ToInt32(reader["InscripcionID"]),
                                TituloCompetencia = reader["TituloCompetencia"].ToString() ?? "",
                                FechaInicio = Convert.ToDateTime(reader["FechaInicio"]),
                                FechaFin = Convert.ToDateTime(reader["FechaFin"]),
                                NombreEquipo = reader["NombreEquipo"] as string,
                                TipoInscripcion = reader["TipoEntradaCompetencia"].ToString() ?? "Inscripción",
                                Precio = reader["PrecioCompetencia"] != DBNull.Value ? Convert.ToDecimal(reader["PrecioCompetencia"]) : 0,
                                CodigoCheckIn = reader["CodigoCheckIn"].ToString() ?? "",
                                Estado = reader["Estado"].ToString() ?? "",
                                PagoID = reader["PagoID"] as int?
                            });
                        }
                    }
                }
            }
            return resultado;
        }

        public (bool Exito, string Mensaje) RegistrarCheckIn(string tipoActividad, int actividadId, string codigoCheckIn)
        {
            using (var conexion = new SqlConnection(_cadenaSQL))
            {
                conexion.Open();
                var cmd = new SqlCommand("sp_RegistrarCheckIn", conexion)
                {
                    CommandType = System.Data.CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@TipoActividad", tipoActividad); 
                cmd.Parameters.AddWithValue("@ActividadID", actividadId);   
                cmd.Parameters.AddWithValue("@CodigoCheckIn", codigoCheckIn);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return (Convert.ToInt32(reader["Exito"]) == 1, reader["Mensaje"].ToString() ?? "Error desconocido.");
                    }
                }
            }
            return (false, "No se pudo procesar el código.");
        }

        public bool VerificarInscripcionExistente(int usuarioId, string tipoActividad, int actividadId)
        {
            using (var conexion = new SqlConnection(_cadenaSQL))
            {
                conexion.Open();

                var sqlBuilder = new StringBuilder("SELECT COUNT(1) FROM Inscripciones WHERE TipoActividad = @tipoActividad AND ActividadID = @actividadId");

                if (usuarioId > 0)
                {
                    sqlBuilder.Append(" AND UsuarioID = @usuarioId");
                }

                var cmd = new SqlCommand(sqlBuilder.ToString(), conexion);
                cmd.Parameters.AddWithValue("@tipoActividad", tipoActividad);
                cmd.Parameters.AddWithValue("@actividadId", actividadId);

                if (usuarioId > 0)
                {
                    cmd.Parameters.AddWithValue("@usuarioId", usuarioId);
                }

                return (int)cmd.ExecuteScalar() > 0;
            }
        }
    }
}
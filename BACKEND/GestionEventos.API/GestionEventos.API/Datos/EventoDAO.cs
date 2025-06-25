// Ubicación: GestionEventos.API/Datos/EventoDAO.cs
using GestionEventos.API.Modelos;
using GestionEventos.API.Modelos.DTOs;
using Microsoft.Data.SqlClient;
using System.Text;
using System.Transactions;

namespace GestionEventos.API.Datos
{
    public class EventoDAO
    {
        private readonly string _cadenaSQL;

        public EventoDAO(string cadenaSQL)
        {
            _cadenaSQL = cadenaSQL;
        }

        public Evento CrearEvento(CrearEventoRequest request)
        {
            int nuevoEventoId;
            using (var conexion = new SqlConnection(_cadenaSQL))
            {
                conexion.Open();
                SqlTransaction transaccion = conexion.BeginTransaction();
                try
                {
                    var sqlEvento = "INSERT INTO Eventos (Titulo, Descripcion, FechaEvento, Ubicacion, OrganizadorID, ImagenUrl, ModalidadPago) VALUES (@titulo, @descripcion, @fechaEvento, @ubicacion, @organizadorID, @imagenUrl, @modalidad); SELECT SCOPE_IDENTITY();";
                    var comandoEvento = new SqlCommand(sqlEvento, conexion, transaccion);
                    comandoEvento.Parameters.AddWithValue("@titulo", request.Titulo);
                    comandoEvento.Parameters.AddWithValue("@descripcion", request.Descripcion);
                    comandoEvento.Parameters.AddWithValue("@fechaEvento", request.FechaEvento);
                    comandoEvento.Parameters.AddWithValue("@ubicacion", request.Ubicacion);
                    comandoEvento.Parameters.AddWithValue("@organizadorID", request.OrganizadorID);
                    comandoEvento.Parameters.AddWithValue("@modalidad", request.Entradas.Any(e => e.Precio > 0) ? "DePago" : "Gratis");
                    comandoEvento.Parameters.Add(new SqlParameter("@imagenUrl", !string.IsNullOrEmpty(request.ImagenUrl) ? request.ImagenUrl : DBNull.Value));

                    var resultadoScalar = comandoEvento.ExecuteScalar();
                    nuevoEventoId = (resultadoScalar == null || resultadoScalar == DBNull.Value) ? 0 : Convert.ToInt32(resultadoScalar);
                    if (nuevoEventoId == 0) throw new Exception("No se pudo obtener el ID del nuevo evento.");

                    foreach (var categoriaId in request.CategoriaIDs)
                    {
                        var sqlCategoria = "INSERT INTO EventoCategorias (EventoID, CategoriaID) VALUES (@eventoID, @categoriaID)";
                        var comandoCategoria = new SqlCommand(sqlCategoria, conexion, transaccion);
                        comandoCategoria.Parameters.AddWithValue("@eventoID", nuevoEventoId);
                        comandoCategoria.Parameters.AddWithValue("@categoriaID", categoriaId);
                        comandoCategoria.ExecuteNonQuery();
                    }

                    foreach (var entrada in request.Entradas)
                    {
                        var sqlEntrada = "INSERT INTO TiposEntrada (EventoID, Nombre, Precio, CantidadTotal, CantidadDisponible, FechaInicioVenta, FechaFinVenta) VALUES (@eventoID, @nombre, @precio, @cantidadTotal, @cantidadDisponible, @fechaInicioVenta, @fechaFinVenta);";
                        var comandoEntrada = new SqlCommand(sqlEntrada, conexion, transaccion);
                        comandoEntrada.Parameters.AddWithValue("@eventoID", nuevoEventoId);
                        comandoEntrada.Parameters.AddWithValue("@nombre", entrada.Nombre);
                        comandoEntrada.Parameters.AddWithValue("@precio", entrada.Precio);
                        comandoEntrada.Parameters.AddWithValue("@cantidadTotal", entrada.CantidadTotal);
                        comandoEntrada.Parameters.AddWithValue("@cantidadDisponible", entrada.CantidadTotal);
                        comandoEntrada.Parameters.AddWithValue("@fechaInicioVenta", entrada.FechaInicioVenta);
                        comandoEntrada.Parameters.AddWithValue("@fechaFinVenta", entrada.FechaFinVenta);
                        comandoEntrada.ExecuteNonQuery();
                    }

                    transaccion.Commit();
                }
                catch (Exception) 
                { 
                    transaccion.Rollback(); 
                    throw; 
                }
            }
            return new Evento { 
                EventoID = nuevoEventoId, 
                Titulo = request.Titulo, 
                Descripcion = request.Descripcion, 
                FechaEvento = request.FechaEvento, 
                Ubicacion = request.Ubicacion, 
                OrganizadorID = request.OrganizadorID, 
                ImagenUrl = request.ImagenUrl, 
                ModalidadPago = request.Entradas.Any(e => e.Precio > 0) ? "DePago" : "Gratis" 
            };
        }

        public List<EventoPublicoDTO> ListarEventosDisponibles()
        {
            var lista = new List<EventoPublicoDTO>();
            using (var conexion = new SqlConnection(_cadenaSQL))
            {
                conexion.Open();
                var comando = new SqlCommand("SELECT EventoID, Titulo, FechaEvento, Ubicacion, Organizador FROM vw_EventosDisponibles", conexion);
                using (var reader = comando.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        lista.Add(new EventoPublicoDTO
                        {
                            EventoID = Convert.ToInt32(reader["EventoID"]),
                            Titulo = reader["Titulo"].ToString() ?? string.Empty,
                            FechaEvento = Convert.ToDateTime(reader["FechaEvento"]),
                            Ubicacion = reader["Ubicacion"].ToString() ?? string.Empty,
                            OrganizadorNombre = reader["Organizador"].ToString() ?? string.Empty
                        });
                    }
                }
            }
            return lista;
        }

        public EventoDetalleDTO? ObtenerEventoPorId(int id)
        {
            EventoDetalleDTO? eventoDetalle = null;
            using (var conexion = new SqlConnection(_cadenaSQL))
            {
                conexion.Open();
                var comandoEvento = new SqlCommand("SELECT * FROM Eventos WHERE EventoID = @id", conexion);
                comandoEvento.Parameters.AddWithValue("@id", id);
                using (var readerEvento = comandoEvento.ExecuteReader())
                {
                    if (readerEvento.Read())
                    {
                        eventoDetalle = new EventoDetalleDTO
                        {
                            EventoID = Convert.ToInt32(readerEvento["EventoID"]),
                            Titulo = readerEvento["Titulo"].ToString() ?? "",
                            Descripcion = readerEvento["Descripcion"].ToString() ?? "",
                            FechaEvento = Convert.ToDateTime(readerEvento["FechaEvento"]),
                            Ubicacion = readerEvento["Ubicacion"].ToString() ?? "",
                            OrganizadorID = Convert.ToInt32(readerEvento["OrganizadorID"]),
                            ImagenUrl = readerEvento["ImagenUrl"] as string,
                            ModalidadPago = readerEvento["ModalidadPago"].ToString() ?? ""
                        };
                    }
                }

                if (eventoDetalle != null)
                {
                    var comandoEntradas = new SqlCommand("SELECT * FROM TiposEntrada WHERE EventoID = @id", conexion);
                    comandoEntradas.Parameters.AddWithValue("@id", id);
                    using (var readerEntradas = comandoEntradas.ExecuteReader())
                    {
                        while (readerEntradas.Read())
                        {
                            eventoDetalle.Entradas.Add(new TiposEntrada
                            {
                                TipoEntradaID = Convert.ToInt32(readerEntradas["TipoEntradaID"]),
                                EventoID = Convert.ToInt32(readerEntradas["EventoID"]),
                                Nombre = readerEntradas["Nombre"].ToString() ?? "",
                                Precio = Convert.ToDecimal(readerEntradas["Precio"]),
                                CantidadTotal = Convert.ToInt32(readerEntradas["CantidadTotal"]),
                                CantidadDisponible = Convert.ToInt32(readerEntradas["CantidadDisponible"]),
                                FechaInicioVenta = Convert.ToDateTime(readerEntradas["FechaInicioVenta"]),
                                FechaFinVenta = Convert.ToDateTime(readerEntradas["FechaFinVenta"])
                            });
                        }
                    }

                    var comandoCategorias = new SqlCommand("SELECT c.CategoriaID, c.Nombre FROM Categorias c JOIN EventoCategorias ec ON c.CategoriaID = ec.CategoriaID WHERE ec.EventoID = @id", conexion);
                    comandoCategorias.Parameters.AddWithValue("@id", id);
                    using (var readerCategorias = comandoCategorias.ExecuteReader())
                    {
                        while (readerCategorias.Read())
                        {
                            eventoDetalle.Categorias.Add(new Categoria { 
                                CategoriaID = Convert.ToInt32(readerCategorias["CategoriaID"]), Nombre = readerCategorias["Nombre"].ToString() ?? "" });
                        }
                    }
                }
            }
            return eventoDetalle;
        }

        public bool ActualizarEvento(ActualizarEventoRequest request)
        {
            using (var conexion = new SqlConnection(_cadenaSQL))
            {
                conexion.Open();
                SqlTransaction transaccion = conexion.BeginTransaction();
                try
                {
                    var sql = "UPDATE Eventos SET Titulo = @titulo, Descripcion = @descripcion, FechaEvento = @fechaEvento, Ubicacion = @ubicacion, ImagenUrl = @imagenUrl WHERE EventoID = @eventoID";
                    var comando = new SqlCommand(sql, conexion, transaccion);
                    comando.Parameters.AddWithValue("@eventoID", request.EventoID);
                    comando.Parameters.AddWithValue("@titulo", request.Titulo);
                    comando.Parameters.AddWithValue("@descripcion", request.Descripcion);
                    comando.Parameters.AddWithValue("@fechaEvento", request.FechaEvento);
                    comando.Parameters.AddWithValue("@ubicacion", request.Ubicacion);
                    comando.Parameters.Add(new SqlParameter("@imagenUrl", !string.IsNullOrEmpty(request.ImagenUrl) ? request.ImagenUrl : DBNull.Value));

                    int filasAfectadas = comando.ExecuteNonQuery();

                    var cmdBorrarCategorias = new SqlCommand("DELETE FROM EventoCategorias WHERE EventoID = @eventoID", conexion, transaccion);
                    cmdBorrarCategorias.Parameters.AddWithValue("@eventoID", request.EventoID);
                    cmdBorrarCategorias.ExecuteNonQuery();

                    foreach (var categoriaId in request.CategoriaIDs)
                    {
                        var sqlCategoria = "INSERT INTO EventoCategorias (EventoID, CategoriaID) VALUES (@eventoID, @categoriaID)";
                        var cmdInsertarCategoria = new SqlCommand(sqlCategoria, conexion, transaccion);
                        cmdInsertarCategoria.Parameters.AddWithValue("@eventoID", request.EventoID);
                        cmdInsertarCategoria.Parameters.AddWithValue("@categoriaID", categoriaId);
                        cmdInsertarCategoria.ExecuteNonQuery();
                    }

                    transaccion.Commit();
                    return filasAfectadas > 0;
                }
                catch (Exception)
                {
                    transaccion.Rollback();
                    return false;
                }
            }
        }

        public bool EliminarEventoFisicamente(int eventoId)
        {
            using (var conexion = new SqlConnection(_cadenaSQL))
            {
                conexion.Open();
                var cmd = new SqlCommand("DELETE FROM Eventos WHERE EventoID = @id", conexion);
                cmd.Parameters.AddWithValue("@id", eventoId);
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        public bool DeshabilitarEventoYReembolsar(int eventoId)
        {
            using (var conexion = new SqlConnection(_cadenaSQL))
            {
                conexion.Open();
                SqlTransaction transaccion = conexion.BeginTransaction();
                try
                {
                    var cmdEvento = new SqlCommand("UPDATE Eventos SET Estado = 'Deshabilitado' WHERE EventoID = @eventoId", conexion, transaccion);
                    cmdEvento.Parameters.AddWithValue("@eventoId", eventoId);
                    cmdEvento.ExecuteNonQuery();

                    var ids = new List<(int InscripcionID, int? PagoID)>();
                    var sqlGetIds = @"SELECT i.InscripcionID, pi.PagoID 
                                    FROM Inscripciones i
                                    LEFT JOIN PagoInscripciones pi ON i.InscripcionID = pi.InscripcionID
                                    WHERE i.ActividadID = @eventoId AND i.TipoActividad = 'Evento'";
                    using (var cmdGetIds = new SqlCommand(sqlGetIds, conexion, transaccion))
                    {
                        cmdGetIds.Parameters.AddWithValue("@eventoId", eventoId);
                        using (var reader = cmdGetIds.ExecuteReader())
                        {
                            while (reader.Read()) { ids.Add((Convert.ToInt32(reader["InscripcionID"]), reader["PagoID"] as int?)); }
                        }
                    }

                    var inscripcionIds = ids.Select(i => i.InscripcionID).ToList();
                    if (inscripcionIds.Any())
                    {
                        var inscripcionDAO = new InscripcionDAO(_cadenaSQL);
                        inscripcionDAO.ActualizarEstadoInscripciones(inscripcionIds, "Cancelado", conexion, transaccion);
                    }

                    var pagoIds = ids.Where(i => i.PagoID.HasValue).Select(i => i.PagoID!.Value).Distinct().ToList();
                    if (pagoIds.Any())
                    {
                        var sqlPagos = $"UPDATE Pagos SET EstadoPago = 'Reembolsado' WHERE PagoID IN ({string.Join(",", pagoIds)})";
                        using (var cmdPagos = new SqlCommand(sqlPagos, conexion, transaccion))
                        {
                            cmdPagos.ExecuteNonQuery();
                        }
                    }

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

        public TiposEntrada? ObtenerTipoEntradaPorId(int tipoEntradaId)
        {
            TiposEntrada? tipoEntrada = null;
            using (var conexion = new SqlConnection(_cadenaSQL))
            {
                conexion.Open();
                var cmd = new SqlCommand("SELECT * FROM TiposEntrada WHERE TipoEntradaID = @id", conexion);
                cmd.Parameters.AddWithValue("@id", tipoEntradaId);
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        tipoEntrada = new TiposEntrada
                        {
                            TipoEntradaID = Convert.ToInt32(reader["TipoEntradaID"]),
                            EventoID = Convert.ToInt32(reader["EventoID"]),
                            Nombre = reader["Nombre"].ToString() ?? "",
                            Precio = Convert.ToDecimal(reader["Precio"]),
                            CantidadTotal = Convert.ToInt32(reader["CantidadTotal"]),
                            CantidadDisponible = Convert.ToInt32(reader["CantidadDisponible"]),
                            FechaInicioVenta = Convert.ToDateTime(reader["FechaInicioVenta"]),
                            FechaFinVenta = Convert.ToDateTime(reader["FechaFinVenta"])
                        };
                    }
                }
            }
            return tipoEntrada;
        }

        public List<Evento> ListarEventosPorOrganizador(int organizadorId)
        {
            var lista = new List<Evento>();
            using (var conexion = new SqlConnection(_cadenaSQL))
            {
                conexion.Open();
                var sql = @"
                    SELECT 
                        e.EventoID,
                        e.Titulo,
                        e.Descripcion,
                        e.FechaEvento,
                        e.Ubicacion,
                        e.OrganizadorID,
                        e.ImagenUrl,
                        e.ModalidadPago,
                        CASE 
                            WHEN e.Estado = 'Deshabilitado' THEN 'Cancelado'
                            WHEN e.FechaEvento < GETDATE() THEN 'Finalizado'
                            ELSE e.Estado
                        END AS EstadoCalculado, 
                        (SELECT COUNT(1) FROM Inscripciones i WHERE i.ActividadID = e.EventoID AND i.TipoActividad = 'Evento' AND i.Estado = 'Confirmado') as CantidadInscritos
                    FROM 
                        Eventos e
                    WHERE 
                        e.OrganizadorID = @organizadorId
                    ORDER BY
                        e.FechaEvento DESC";

                var cmd = new SqlCommand(sql, conexion);
                cmd.Parameters.AddWithValue("@organizadorId", organizadorId);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        lista.Add(new Evento
                        {
                            EventoID = Convert.ToInt32(reader["EventoID"]),
                            Titulo = reader["Titulo"].ToString() ?? "",
                            FechaEvento = Convert.ToDateTime(reader["FechaEvento"]),
                            Ubicacion = reader["Ubicacion"].ToString() ?? "",
                            OrganizadorID = Convert.ToInt32(reader["OrganizadorID"]),
                            ImagenUrl = reader["ImagenUrl"] as string,
                            ModalidadPago = reader["ModalidadPago"].ToString() ?? "",
                            Estado = reader["EstadoCalculado"].ToString() ?? "Activo",
                            CantidadInscritos = Convert.ToInt32(reader["CantidadInscritos"]) 
                        });
                    }
                }
            }
            return lista;
        }

        private void ActualizarEstadosDeEventosPasados()
        {
            using (var conexion = new SqlConnection(_cadenaSQL))
            {
                conexion.Open();
                var sql = "UPDATE Eventos SET Estado = 'Finalizado' WHERE FechaEvento < GETDATE() AND Estado = 'Activo'";
                using (var cmd = new SqlCommand(sql, conexion))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public List<EventoPublicoDTO> ListarEventosPublicos(int? categoriaId = null)
        {
            ActualizarEstadosDeEventosPasados();
            var lista = new List<EventoPublicoDTO>();
            using (var conexion = new SqlConnection(_cadenaSQL))
            {
                conexion.Open();

                var sqlBuilder = new StringBuilder(@"
                    SELECT DISTINCT
                        e.EventoID,
                        e.Titulo,
                        e.FechaEvento,
                        e.Ubicacion,
                        e.ImagenUrl,
                        e.Estado,
                        e.ModalidadPago,
                        u.Nombre as OrganizadorNombre,
                        (SELECT STRING_AGG(cat.Nombre, ', ') 
                         FROM EventoCategorias ec_inner 
                         JOIN Categorias cat ON ec_inner.CategoriaID = cat.CategoriaID 
                         WHERE ec_inner.EventoID = e.EventoID) AS Categorias,
                        COALESCE((SELECT MIN(Precio) FROM TiposEntrada WHERE EventoID = e.EventoID), 0) AS PrecioDesde
                    FROM 
                        Eventos e
                    JOIN 
                        Usuarios u ON e.OrganizadorID = u.UsuarioID");

                var whereClauses = new List<string>
                {
                    "e.Estado = 'Activo'", 
                    "e.FechaEvento >= GETDATE()" 
                };

                if (categoriaId.HasValue && categoriaId.Value > 0)
                {
                    sqlBuilder.Append(" JOIN EventoCategorias ec_filter ON e.EventoID = ec_filter.EventoID");
                    whereClauses.Add("ec_filter.CategoriaID = @categoriaId");
                }

                if (whereClauses.Any())
                {
                    sqlBuilder.Append(" WHERE " + string.Join(" AND ", whereClauses));
                }

                sqlBuilder.Append(" ORDER BY e.FechaEvento ASC");

                var cmd = new SqlCommand(sqlBuilder.ToString(), conexion);

                if (categoriaId.HasValue && categoriaId.Value > 0)
                {
                    cmd.Parameters.AddWithValue("@categoriaId", categoriaId.Value);
                }
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read()) {
                        lista.Add(new EventoPublicoDTO
                        {
                            EventoID = Convert.ToInt32(reader["EventoID"]),
                            Titulo = reader["Titulo"].ToString() ?? "",
                            FechaEvento = Convert.ToDateTime(reader["FechaEvento"]),
                            Ubicacion = reader["Ubicacion"].ToString() ?? "",
                            OrganizadorNombre = reader["OrganizadorNombre"].ToString() ?? "",
                            Categorias = reader["Categorias"].ToString() ?? "",
                            ImagenUrl = reader["ImagenUrl"] as string,
                            Estado = reader["Estado"].ToString() ?? string.Empty,
                            ModalidadPago = reader["ModalidadPago"].ToString() ?? ""
                        });
                    }
                }
            }
            return lista;
        }
    }
}
using GestionEventos.API.Modelos;
using GestionEventos.API.Modelos.DTOs;
using Microsoft.Data.SqlClient;
using System.Text;

namespace GestionEventos.API.Datos
{
    public class CompetenciaDAO
    {
        private readonly string _cadenaSQL;
        public CompetenciaDAO(string cadenaSQL) 
        { 
            _cadenaSQL = cadenaSQL; 
        }

        public Competencia CrearCompetencia(CrearCompetenciaRequest request)
        {
            int nuevaCompetenciaId;
            using (var conexion = new SqlConnection(_cadenaSQL))
            {
                conexion.Open();
                SqlTransaction transaccion = conexion.BeginTransaction();
                try
                {
                    string modalidadPago = request.Entradas.Any(e => e.Precio > 0) ? "DePago" : "Gratis";
                    var sql = "INSERT INTO Competencias (Titulo, Descripcion, FechaInicio, FechaFin, Tipo, TamanoMinEquipo, TamanoMaxEquipo, OrganizadorID, ImagenUrl, ModalidadPago) " +
                              "VALUES (@titulo, @descripcion, @fechaInicio, @fechaFin, @tipo, @min, @max, @organizadorID, @imagenUrl, @modalidad); SELECT SCOPE_IDENTITY();";
                    var cmd = new SqlCommand(sql, conexion, transaccion);
                    cmd.Parameters.Add(new SqlParameter("@titulo", request.Titulo));
                    cmd.Parameters.Add(new SqlParameter("@descripcion", request.Descripcion));
                    cmd.Parameters.Add(new SqlParameter("@fechaInicio", request.FechaInicio));
                    cmd.Parameters.Add(new SqlParameter("@fechaFin", request.FechaFin));
                    cmd.Parameters.Add(new SqlParameter("@tipo", request.Tipo));
                    cmd.Parameters.Add(new SqlParameter("@min", request.TamanoMinEquipo.HasValue ? request.TamanoMinEquipo.Value : DBNull.Value));
                    cmd.Parameters.Add(new SqlParameter("@max", request.TamanoMaxEquipo.HasValue ? request.TamanoMaxEquipo.Value : DBNull.Value));
                    cmd.Parameters.Add(new SqlParameter("@organizadorID", request.OrganizadorID));
                    cmd.Parameters.Add(new SqlParameter("@modalidad", modalidadPago));
                    cmd.Parameters.Add(new SqlParameter("@imagenUrl", !string.IsNullOrEmpty(request.ImagenUrl) ? request.ImagenUrl : DBNull.Value));

                    var result = cmd.ExecuteScalar();
                    nuevaCompetenciaId = (result == null || result == DBNull.Value) ? 0 : Convert.ToInt32(result);
                    if (nuevaCompetenciaId == 0) throw new Exception("No se pudo crear la competencia.");

                    foreach (var categoriaId in request.CategoriaIDs)
                    {
                        var sqlCategoria = "INSERT INTO CompetenciaCategorias (CompetenciaID, CategoriaID) VALUES (@competenciaID, @categoriaID)";
                        var comandoCategoria = new SqlCommand(sqlCategoria, conexion, transaccion);
                        comandoCategoria.Parameters.AddWithValue("@competenciaID", nuevaCompetenciaId);
                        comandoCategoria.Parameters.AddWithValue("@categoriaID", categoriaId);
                        comandoCategoria.ExecuteNonQuery();
                    }

                    foreach (var entrada in request.Entradas)
                    {
                        var sqlEntrada = "INSERT INTO TiposEntradaCompetencia (CompetenciaID, Nombre, Precio, CantidadTotal, CantidadDisponible, FechaInicioVenta, FechaFinVenta) VALUES (@id, @nombre, @precio, @cantidad, @cantidad, @inicioVenta, @finVenta)";
                        var cmdEntrada = new SqlCommand(sqlEntrada, conexion, transaccion);
                        cmdEntrada.Parameters.AddWithValue("@id", nuevaCompetenciaId);
                        cmdEntrada.Parameters.AddWithValue("@nombre", entrada.Nombre);
                        cmdEntrada.Parameters.AddWithValue("@precio", entrada.Precio);
                        cmdEntrada.Parameters.AddWithValue("@cantidad", entrada.CantidadTotal); 
                        cmdEntrada.Parameters.AddWithValue("@inicioVenta", entrada.FechaInicioVenta);
                        cmdEntrada.Parameters.AddWithValue("@finVenta", entrada.FechaFinVenta);
                        cmdEntrada.ExecuteNonQuery();
                    }
                    transaccion.Commit();
                }
                catch (Exception) { transaccion.Rollback(); throw; }
            }
            return new Competencia { 
                CompetenciaID = nuevaCompetenciaId, 
                Titulo = request.Titulo,
                Descripcion = request.Descripcion,
                FechaInicio = request.FechaInicio,
                FechaFin = request.FechaFin,
                Tipo = request.Tipo,
                TamanoMinEquipo = request.TamanoMinEquipo,
                TamanoMaxEquipo = request.TamanoMaxEquipo,
                OrganizadorID = request.OrganizadorID,
                ModalidadPago = request.Entradas.Any(e => e.Precio > 0) ? "DePago" : "Gratis",
                ImagenUrl = request.ImagenUrl 
            };
        }

        private void ActualizarEstadosDeCompetenciasPasadas()
        {
            using (var conexion = new SqlConnection(_cadenaSQL))
            {
                conexion.Open();
                var sql = "UPDATE Competencias SET Estado = 'Finalizado' WHERE FechaFin < GETDATE() AND Estado = 'Activo'";
                using (var cmd = new SqlCommand(sql, conexion))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public List<CompetenciaPublicaDTO> ListarCompetenciasPublicas(int? categoriaId = null)
        {
            ActualizarEstadosDeCompetenciasPasadas();
            var lista = new List<CompetenciaPublicaDTO>();
            using (var conexion = new SqlConnection(_cadenaSQL))
            {
                conexion.Open();

                var sqlBuilder = new StringBuilder(@"
                    SELECT DISTINCT 
                        c.CompetenciaID, c.Titulo, c.FechaInicio, c.FechaFin, c.Tipo, c.ImagenUrl, c.Estado, c.ModalidadPago, 
                        u.Nombre as OrganizadorNombre,
                        (SELECT STRING_AGG(cat.Nombre, ', ') FROM CompetenciaCategorias cc_inner JOIN Categorias cat ON cc_inner.CategoriaID = cat.CategoriaID WHERE cc_inner.CompetenciaID = c.CompetenciaID) AS Categorias,
                        COALESCE((SELECT MIN(Precio) FROM TiposEntradaCompetencia WHERE CompetenciaID = c.CompetenciaID), 0) AS PrecioDesde
                    FROM Competencias c
                    JOIN Usuarios u ON c.OrganizadorID = u.UsuarioID");

                var whereClauses = new List<string> { "c.Estado = 'Activo'", "c.FechaFin >= GETDATE()" };

                if (categoriaId.HasValue && categoriaId.Value > 0)
                {
                    sqlBuilder.Append(" JOIN CompetenciaCategorias cc_filter ON c.CompetenciaID = cc_filter.CompetenciaID");
                    whereClauses.Add("cc_filter.CategoriaID = @categoriaId");
                }

                if (whereClauses.Any())
                {
                    sqlBuilder.Append(" WHERE " + string.Join(" AND ", whereClauses));
                }

                sqlBuilder.Append(" ORDER BY c.FechaInicio ASC");

                using (var cmd = new SqlCommand(sqlBuilder.ToString(), conexion))
                {
                    if (categoriaId.HasValue && categoriaId.Value > 0)
                    {
                        cmd.Parameters.AddWithValue("@categoriaId", categoriaId.Value);
                    }

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lista.Add(new CompetenciaPublicaDTO
                            {
                                CompetenciaID = Convert.ToInt32(reader["CompetenciaID"]),
                                Titulo = reader["Titulo"].ToString() ?? "",
                                FechaInicio = Convert.ToDateTime(reader["FechaInicio"]),
                                FechaFin = Convert.ToDateTime(reader["FechaFin"]),
                                Tipo = reader["Tipo"].ToString() ?? "",
                                OrganizadorNombre = reader["OrganizadorNombre"].ToString() ?? "",
                                ImagenUrl = reader["ImagenUrl"] as string,
                                Categorias = reader["Categorias"] as string,
                                Estado = reader["Estado"].ToString() ?? "Activo",
                                ModalidadPago = reader["ModalidadPago"].ToString() ?? "Gratis"
                            });
                        }
                    }
                }
            } 
            return lista;
        }

        public CompetenciaDetalleDTO? ObtenerCompetenciaPorId(int id)
        {
            CompetenciaDetalleDTO? competenciaDetalle = null;
            using (var conexion = new SqlConnection(_cadenaSQL))
            {
                conexion.Open();
                var cmd = new SqlCommand("SELECT * FROM Competencias WHERE CompetenciaID = @id", conexion);
                cmd.Parameters.AddWithValue("@id", id);
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        competenciaDetalle = new CompetenciaDetalleDTO
                        {
                            CompetenciaID = Convert.ToInt32(reader["CompetenciaID"]),
                            Titulo = reader["Titulo"].ToString() ?? "",
                            Descripcion = reader["Descripcion"].ToString() ?? "",
                            FechaInicio = Convert.ToDateTime(reader["FechaInicio"]),
                            FechaFin = Convert.ToDateTime(reader["FechaFin"]),
                            Tipo = reader["Tipo"].ToString() ?? "",
                            TamanoMinEquipo = reader["TamanoMinEquipo"] as int?,
                            TamanoMaxEquipo = reader["TamanoMaxEquipo"] as int?,
                            OrganizadorID = Convert.ToInt32(reader["OrganizadorID"]),
                            ImagenUrl = reader["ImagenUrl"] as string,
                            ModalidadPago = reader["ModalidadPago"].ToString() ?? "Gratis"
                        };
                    }
                }
                if (competenciaDetalle != null)
                {
                    var cmdCategorias = new SqlCommand("SELECT c.CategoriaID, c.Nombre FROM Categorias c JOIN CompetenciaCategorias cc ON c.CategoriaID = cc.CategoriaID WHERE cc.CompetenciaID = @id", conexion);
                    cmdCategorias.Parameters.AddWithValue("@id", id);
                    using (var reader = cmdCategorias.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            competenciaDetalle.Categorias.Add(new Categoria { CategoriaID = Convert.ToInt32(reader["CategoriaID"]), Nombre = reader["Nombre"].ToString() ?? "" });
                        }
                    }

                    var cmdEntradas = new SqlCommand("SELECT * FROM TiposEntradaCompetencia WHERE CompetenciaID = @id", conexion);
                    cmdEntradas.Parameters.AddWithValue("@id", id);
                    using (var reader = cmdEntradas.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            competenciaDetalle.Entradas.Add(new TiposEntradaCompetencia
                            {
                                TipoEntradaID = Convert.ToInt32(reader["TipoEntradaID"]),
                                CompetenciaID = Convert.ToInt32(reader["CompetenciaID"]),
                                Nombre = reader["Nombre"].ToString() ?? "",
                                Precio = Convert.ToDecimal(reader["Precio"]),
                                CantidadTotal = Convert.ToInt32(reader["CantidadTotal"]),
                                CantidadDisponible = Convert.ToInt32(reader["CantidadDisponible"]),
                                FechaInicioVenta = Convert.ToDateTime(reader["FechaInicioVenta"]),
                                FechaFinVenta = Convert.ToDateTime(reader["FechaFinVenta"])
                            });
                        }
                    }
                }
            }
            return competenciaDetalle;
        }


        public bool ActualizarCompetencia(ActualizarCompetenciaRequest request)
        {
            using (var conexion = new SqlConnection(_cadenaSQL))
            {
                conexion.Open();
                SqlTransaction transaccion = conexion.BeginTransaction();
                try
                {
                    string modalidadPago = request.Entradas.Any(e => e.Precio > 0) ? "DePago" : "Gratis";
                    var sql = "UPDATE Competencias SET Titulo = @titulo, Descripcion = @descripcion, FechaInicio = @fechaInicio, " +
                              "FechaFin = @fechaFin, Tipo = @tipo, TamanoMinEquipo = @min, TamanoMaxEquipo = @max, ImagenUrl = @imagenUrl, ModalidadPago = @modalidad " +
                              "WHERE CompetenciaID = @id";
                    var cmd = new SqlCommand(sql, conexion, transaccion);
                    cmd.Parameters.Add(new SqlParameter("@id", request.CompetenciaID));
                    cmd.Parameters.Add(new SqlParameter("@titulo", request.Titulo));
                    cmd.Parameters.Add(new SqlParameter("@descripcion", request.Descripcion));
                    cmd.Parameters.Add(new SqlParameter("@fechaInicio", request.FechaInicio));
                    cmd.Parameters.Add(new SqlParameter("@fechaFin", request.FechaFin));
                    cmd.Parameters.Add(new SqlParameter("@tipo", request.Tipo));
                    cmd.Parameters.Add(new SqlParameter("@min", request.TamanoMinEquipo.HasValue ? request.TamanoMinEquipo.Value : DBNull.Value));
                    cmd.Parameters.Add(new SqlParameter("@max", request.TamanoMaxEquipo.HasValue ? request.TamanoMaxEquipo.Value : DBNull.Value));
                    cmd.Parameters.Add(new SqlParameter("@modalidad", modalidadPago));
                    cmd.Parameters.Add(new SqlParameter("@imagenUrl", !string.IsNullOrEmpty(request.ImagenUrl) ? request.ImagenUrl : DBNull.Value));
                    int filasAfectadas = cmd.ExecuteNonQuery();

                    var cmdBorrarCategorias = new SqlCommand("DELETE FROM CompetenciaCategorias WHERE CompetenciaID = @id", conexion, transaccion);
                    cmdBorrarCategorias.Parameters.AddWithValue("@id", request.CompetenciaID);
                    cmdBorrarCategorias.ExecuteNonQuery();
                    foreach (var categoriaId in request.CategoriaIDs)
                    {
                        var sqlCategoria = "INSERT INTO CompetenciaCategorias (CompetenciaID, CategoriaID) VALUES (@id, @categoriaId)";
                        var cmdInsertarCategoria = new SqlCommand(sqlCategoria, conexion, transaccion);
                        cmdInsertarCategoria.Parameters.AddWithValue("@id", request.CompetenciaID);
                        cmdInsertarCategoria.Parameters.AddWithValue("@categoriaId", categoriaId);
                        cmdInsertarCategoria.ExecuteNonQuery();
                    }

                    var cmdBorrarEntradas = new SqlCommand("DELETE FROM TiposEntradaCompetencia WHERE CompetenciaID = @id", conexion, transaccion);
                    cmdBorrarEntradas.Parameters.AddWithValue("@id", request.CompetenciaID);
                    cmdBorrarEntradas.ExecuteNonQuery();
                    foreach (var entrada in request.Entradas)
                    {
                        var sqlEntrada = "INSERT INTO TiposEntradaCompetencia (CompetenciaID, Nombre, Precio, CantidadTotal, CantidadDisponible, FechaInicioVenta, FechaFinVenta) VALUES (@id, @nombre, @precio, @cantidad, @cantidad, @inicioVenta, @finVenta)";
                        var cmdEntrada = new SqlCommand(sqlEntrada, conexion, transaccion);
                        cmdEntrada.Parameters.AddWithValue("@id", request.CompetenciaID);
                        cmdEntrada.Parameters.AddWithValue("@nombre", entrada.Nombre);
                        cmdEntrada.Parameters.AddWithValue("@precio", entrada.Precio);
                        cmdEntrada.Parameters.AddWithValue("@cantidad", entrada.CantidadTotal); 
                        cmdEntrada.Parameters.AddWithValue("@inicioVenta", entrada.FechaInicioVenta);
                        cmdEntrada.Parameters.AddWithValue("@finVenta", entrada.FechaFinVenta);
                        cmdEntrada.ExecuteNonQuery();
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

        public bool EliminarCompetenciaFisicamente(int competenciaId)
        {
            using (var conexion = new SqlConnection(_cadenaSQL))
            {
                conexion.Open();
                var cmd = new SqlCommand("DELETE FROM Competencias WHERE CompetenciaID = @id", conexion);
                cmd.Parameters.AddWithValue("@id", competenciaId);
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        public bool DeshabilitarCompetenciaYReembolsar(int competenciaId)
        {
            using (var conexion = new SqlConnection(_cadenaSQL))
            {
                conexion.Open();
                SqlTransaction transaccion = conexion.BeginTransaction();
                try
                {
                    var cmdComp = new SqlCommand("UPDATE Competencias SET Estado = 'Deshabilitado' WHERE CompetenciaID = @id", conexion, transaccion);
                    cmdComp.Parameters.AddWithValue("@id", competenciaId);
                    cmdComp.ExecuteNonQuery();

                    var ids = new List<(int InscripcionID, int? PagoID)>();
                    var sqlGetIds = @"SELECT i.InscripcionID, pi.PagoID FROM Inscripciones i LEFT JOIN PagoInscripciones pi ON i.InscripcionID = pi.InscripcionID WHERE i.ActividadID = @id AND i.TipoActividad = 'Competencia'";
                    using (var cmdGetIds = new SqlCommand(sqlGetIds, conexion, transaccion))
                    {
                        cmdGetIds.Parameters.AddWithValue("@id", competenciaId);
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
                        var equipoDAO = new EquipoDAO(_cadenaSQL);
                        equipoDAO.ActualizarEstadoEquipoPorInscripciones(inscripcionIds, "Cancelado", conexion, transaccion);
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

        public List<Competencia> ListarCompetenciasPorOrganizador(int organizadorId)
        {
            var lista = new List<Competencia>();
            using (var conexion = new SqlConnection(_cadenaSQL))
            {
                conexion.Open();
                var sql = @"
                    SELECT 
                        c.*,
                        CASE 
                            WHEN c.Estado = 'Deshabilitado' THEN 'Cancelado'
                            WHEN c.FechaFin < GETDATE() THEN 'Finalizado'
                            ELSE c.Estado
                        END AS EstadoCalculado, 
                        (SELECT COUNT(DISTINCT i.UsuarioID) FROM Inscripciones i WHERE i.ActividadID = c.CompetenciaID AND i.TipoActividad = 'Competencia' AND i.Estado = 'Confirmado') as CantidadInscritos
                    FROM Competencias c
                    WHERE c.OrganizadorID = @organizadorId
                    ORDER BY c.FechaInicio DESC";
                var cmd = new SqlCommand(sql, conexion);
                cmd.Parameters.AddWithValue("@organizadorId", organizadorId);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        lista.Add(new Competencia
                        {
                            CompetenciaID = Convert.ToInt32(reader["CompetenciaID"]),
                            Titulo = reader["Titulo"].ToString() ?? "",
                            Descripcion = reader["Descripcion"].ToString() ?? "",
                            FechaInicio = Convert.ToDateTime(reader["FechaInicio"]),
                            FechaFin = Convert.ToDateTime(reader["FechaFin"]),
                            Tipo = reader["Tipo"].ToString() ?? "",
                            TamanoMinEquipo = reader["TamanoMinEquipo"] != DBNull.Value ? Convert.ToInt32(reader["TamanoMinEquipo"]) : null,
                            TamanoMaxEquipo = reader["TamanoMaxEquipo"] != DBNull.Value ? Convert.ToInt32(reader["TamanoMaxEquipo"]) : null,
                            OrganizadorID = Convert.ToInt32(reader["OrganizadorID"]),
                            ModalidadPago = reader["ModalidadPago"].ToString() ?? "",
                            Estado = reader["Estado"].ToString() ?? "",
                            CantidadInscritos = Convert.ToInt32(reader["CantidadInscritos"])
                        });
                    }
                }
            }
            return lista;
        }     
    }
}
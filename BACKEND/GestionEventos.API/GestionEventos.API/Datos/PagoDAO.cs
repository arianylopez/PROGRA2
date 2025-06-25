using GestionEventos.API.Modelos;
using GestionEventos.API.Modelos.DTOs;
using Microsoft.Data.SqlClient;

namespace GestionEventos.API.Datos
{
    public class PagoDAO
    {
        private readonly string _cadenaSQL;
        public PagoDAO(string cadenaSQL) { _cadenaSQL = cadenaSQL; }

        public (Pago? Pago, List<Inscripcion> Inscripciones) CrearPagoPendiente(int usuarioId, decimal montoTotal, List<Inscripcion> inscripciones, Equipo? equipo = null)
        {
            using (var conexion = new SqlConnection(_cadenaSQL))
            {
                conexion.Open();
                SqlTransaction transaccion = conexion.BeginTransaction();
                try
                {
                    var inscripcionDAO = new InscripcionDAO(_cadenaSQL);
                    var equipoDAO = new EquipoDAO(_cadenaSQL);
                    int? equipoId = null;

                    if (equipo != null)
                    {
                        var equipoCreado = equipoDAO.CrearEquipo(equipo, conexion, transaccion);
                        equipoId = equipoCreado.EquipoID;

                        var miembros = inscripciones.Select(i => new Usuario { UsuarioID = i.UsuarioID }).ToList();
                        equipoDAO.AgregarMiembros(equipoCreado.EquipoID, miembros, conexion, transaccion);
                    }

                    foreach (var i in inscripciones) { i.EquipoID = equipoId; }

                    var inscripcionesCreadas = inscripcionDAO.CrearInscripciones(inscripciones, conexion, transaccion);
                    if (!inscripcionesCreadas.Any()) throw new Exception("No se pudo crear ninguna inscripción reservada.");

                    var sqlPago = "INSERT INTO Pagos (UsuarioID, MontoTotal, FechaPago, EstadoPago) VALUES (@usuarioId, @monto, GETDATE(), 'Pendiente'); SELECT SCOPE_IDENTITY();";
                    var cmdPago = new SqlCommand(sqlPago, conexion, transaccion);
                    cmdPago.Parameters.AddWithValue("@usuarioId", usuarioId);
                    cmdPago.Parameters.AddWithValue("@monto", montoTotal);
                    int nuevoPagoId = Convert.ToInt32(cmdPago.ExecuteScalar());

                    foreach (var inscripcion in inscripcionesCreadas)
                    {
                        var sqlEnlaceInsc = "INSERT INTO PagoInscripciones (PagoID, InscripcionID) VALUES (@pagoId, @inscripcionId)";
                        var cmdEnlaceInsc = new SqlCommand(sqlEnlaceInsc, conexion, transaccion);
                        cmdEnlaceInsc.Parameters.AddWithValue("@pagoId", nuevoPagoId);
                        cmdEnlaceInsc.Parameters.AddWithValue("@inscripcionId", inscripcion.InscripcionID);
                        cmdEnlaceInsc.ExecuteNonQuery();
                    }

                    transaccion.Commit();
                    return (new Pago { PagoID = nuevoPagoId, MontoTotal = montoTotal, EstadoPago = "Pendiente" }, inscripcionesCreadas);
                }
                catch (Exception)
                {
                    transaccion.Rollback();
                    return (null, new List<Inscripcion>());
                }
            }
        }

        public bool ConfirmarPago(ConfirmarPagoRequest request)
        {
            using (var conexion = new SqlConnection(_cadenaSQL))
            {
                conexion.Open();
                SqlTransaction transaccion = conexion.BeginTransaction();
                try
                {
                    var sqlPago = "UPDATE Pagos SET EstadoPago = 'Completado', TransaccionID = @transaccionId, MetodoPagoSimulado = @metodo, FechaPago = GETDATE() WHERE PagoID = @pagoId AND EstadoPago = 'Pendiente'";
                    using (var cmdPago = new SqlCommand(sqlPago, conexion, transaccion))
                    {
                        cmdPago.Parameters.AddWithValue("@pagoId", request.PagoID);
                        cmdPago.Parameters.AddWithValue("@transaccionId", request.TransaccionSimuladaID);
                        cmdPago.Parameters.AddWithValue("@metodo", request.MetodoPagoSimulado);
                        if (cmdPago.ExecuteNonQuery() == 0)
                        {
                            transaccion.Rollback();
                            return false;
                        }
                    }

                    var inscripcionIds = new List<int>();
                    var sqlGetInscripciones = "SELECT InscripcionID FROM PagoInscripciones WHERE PagoID = @pagoId";
                    using (var cmdGetInscripciones = new SqlCommand(sqlGetInscripciones, conexion, transaccion))
                    {
                        cmdGetInscripciones.Parameters.AddWithValue("@pagoId", request.PagoID);
                        using (var reader = cmdGetInscripciones.ExecuteReader())
                        {
                            while (reader.Read()) { inscripcionIds.Add(Convert.ToInt32(reader["InscripcionID"])); }
                        }
                    }

                    if (inscripcionIds.Any())
                    {
                        var inscripcionDAO = new InscripcionDAO(_cadenaSQL);
                        inscripcionDAO.ActualizarEstadoInscripciones(inscripcionIds, "Confirmado", conexion, transaccion);
                    }

                    int? equipoId = null;
                    if (inscripcionIds.Any())
                    {
                        var sqlGetEquipoId = $"SELECT TOP 1 EquipoID FROM Inscripciones WHERE InscripcionID IN ({string.Join(",", inscripcionIds)}) AND EquipoID IS NOT NULL";
                        using (var cmdGetEquipo = new SqlCommand(sqlGetEquipoId, conexion, transaccion))
                        {
                            var result = cmdGetEquipo.ExecuteScalar();
                            if (result != null && result != DBNull.Value)
                            {
                                equipoId = Convert.ToInt32(result);
                            }
                        }
                    }

                    if (equipoId.HasValue)
                    {
                        var equipoDAO = new EquipoDAO(_cadenaSQL);
                        equipoDAO.ActualizarEstadoEquipo(equipoId.Value, "Confirmado", conexion, transaccion);
                    }

                    transaccion.Commit();
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ERROR en ConfirmarPago: {ex.ToString()}");
                    transaccion.Rollback();
                    return false;
                }
            }
        }

        public PagoDetalleDTO? ObtenerDetalleDelPago(int pagoId)
        {
            PagoDetalleDTO? detalle = null;
            using (var conexion = new SqlConnection(_cadenaSQL))
            {
                conexion.Open();
                var sql = @"
                    SELECT TOP 1 
                        p.PagoID, p.MontoTotal,
                        COALESCE(e.Titulo, c.Titulo) as NombreActividad
                    FROM Pagos p
                    LEFT JOIN PagoInscripciones pi ON p.PagoID = pi.PagoID
                    LEFT JOIN Inscripciones i ON pi.InscripcionID = i.InscripcionID
                    LEFT JOIN Eventos e ON i.ActividadID = e.EventoID AND i.TipoActividad = 'Evento'
                    LEFT JOIN Competencias c ON i.ActividadID = c.CompetenciaID AND i.TipoActividad = 'Competencia'
                    WHERE p.PagoID = @pagoId;

                    -- Luego obtenemos los items (entradas de evento o competencia)
                    SELECT 
                        COALESCE(te.Nombre, tec.Nombre) AS Descripcion,
                        COUNT(i.InscripcionID) AS Cantidad,
                        COALESCE(te.Precio, tec.Precio) AS PrecioUnitario
                    FROM Pagos p
                    JOIN PagoInscripciones pi ON p.PagoID = pi.PagoID
                    JOIN Inscripciones i ON pi.InscripcionID = i.InscripcionID
                    LEFT JOIN TiposEntrada te ON i.TipoEntradaID = te.TipoEntradaID
                    LEFT JOIN TiposEntradaCompetencia tec ON i.TipoEntradaID = tec.TipoEntradaID AND i.TipoActividad = 'Competencia'
                    WHERE p.PagoID = @pagoId
                    GROUP BY COALESCE(te.Nombre, tec.Nombre), COALESCE(te.Precio, tec.Precio);";

                var cmd = new SqlCommand(sql, conexion);
                cmd.Parameters.AddWithValue("@pagoId", pagoId);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        detalle = new PagoDetalleDTO
                        {
                            PagoID = Convert.ToInt32(reader["PagoID"]),
                            MontoTotal = Convert.ToDecimal(reader["MontoTotal"]),
                            NombreActividad = reader["NombreActividad"].ToString() ?? ""
                        };
                    }

                    // Pasamos al segundo resultado (los items)
                    if (detalle != null && reader.NextResult())
                    {
                        while (reader.Read())
                        {
                            var item = new PagoItemDTO
                            {
                                Descripcion = reader["Descripcion"].ToString() ?? "",
                                Cantidad = Convert.ToInt32(reader["Cantidad"]),
                                PrecioUnitario = Convert.ToDecimal(reader["PrecioUnitario"])
                            };
                            item.Subtotal = item.Cantidad * item.PrecioUnitario;
                            detalle.Items.Add(item);
                        }
                    }
                }
            }
            return detalle;
        }
    }
}
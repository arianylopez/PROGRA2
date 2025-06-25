using GestionEventos.API.Modelos.DTOs.Dashboard;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Dynamic;

namespace GestionEventos.API.Datos
{
    public class DashboardDAO
    {
        private readonly string _cadenaSQL;
        public DashboardDAO(string cadenaSQL) { _cadenaSQL = cadenaSQL; }

        public DashboardEventoDTO? GetDashboardEvento(int eventoId, int organizadorId)
        {
            var dashboard = new DashboardEventoDTO();
            using (var conexion = new SqlConnection(_cadenaSQL))
            {
                conexion.Open();
                var cmd = new SqlCommand("sp_GetDashboardEvento", conexion) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.AddWithValue("@EventoID", eventoId);
                cmd.Parameters.AddWithValue("@OrganizadorID", organizadorId);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        dashboard.TituloEvento = reader["TituloEvento"].ToString() ?? "";
                        dashboard.TotalInscritos = reader["TotalInscritos"] != DBNull.Value ? Convert.ToInt32(reader["TotalInscritos"]) : 0;
                        dashboard.TotalAsistentes = reader["TotalAsistentes"] != DBNull.Value ? Convert.ToInt32(reader["TotalAsistentes"]) : 0;
                        dashboard.TotalRecaudado = reader["TotalRecaudado"] != DBNull.Value ? Convert.ToDecimal(reader["TotalRecaudado"]) : 0;
                    }
                    else 
                    { 
                        return null; 
                    }

                    if (reader.NextResult())
                    {
                        while (reader.Read())
                        {
                            dashboard.DetalleVentas.Add(new DashboardRenglonVentaDTO
                            {
                                TipoEntrada = reader["TipoEntrada"].ToString() ?? string.Empty,
                                Precio = Convert.ToDecimal(reader["Precio"]),
                                CantidadVendida = Convert.ToInt32(reader["CantidadVendida"]),
                                Subtotal = Convert.ToDecimal(reader["Subtotal"])
                            });
                        }
                    }

                    if (reader.NextResult())
                    {
                        while (reader.Read())
                        {
                            dashboard.Participantes.Add(new DashboardParticipanteDTO
                            {
                                NombreParticipante = reader["NombreParticipante"].ToString() ?? string.Empty,
                                Email = reader["Email"].ToString() ?? string.Empty,
                                CodigoCheckIn = reader["CodigoCheckIn"].ToString() ?? string.Empty,
                                FechaCheckIn = reader["FechaCheckIn"] as DateTime?
                            });
                        }
                    }
                }
            }
            return dashboard;
        }

        public DashboardCompetenciaDTO? GetDashboardCompetencia(int competenciaId, int organizadorId)
        {
            var dashboard = new DashboardCompetenciaDTO();
            using (var conexion = new SqlConnection(_cadenaSQL))
            {
                conexion.Open();
                var cmd = new SqlCommand("sp_GetDashboardCompetencia", conexion)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@CompetenciaID", competenciaId);
                cmd.Parameters.AddWithValue("@OrganizadorID", organizadorId);

                using (var reader = cmd.ExecuteReader())
                {
                    //  Resumen General
                    if (reader.Read())
                    {
                        dashboard.TituloCompetencia = reader["TituloCompetencia"].ToString() ?? "";
                        dashboard.TipoCompetencia = reader["TipoCompetencia"].ToString() ?? "";
                        dashboard.ModalidadPago = reader["ModalidadPago"].ToString() ?? "";
                        dashboard.TotalInscritos = reader["TotalInscritos"] != DBNull.Value ? Convert.ToInt32(reader["TotalInscritos"]) : 0;
                        dashboard.TotalAsistentes = reader["TotalAsistentes"] != DBNull.Value ? Convert.ToInt32(reader["TotalAsistentes"]) : 0;
                        dashboard.TotalRecaudado = reader["TotalRecaudado"] != DBNull.Value ? Convert.ToDecimal(reader["TotalRecaudado"]) : 0;
                    }
                    else { return null; }

                    // Detalle de Ventas
                    reader.NextResult();
                    while (reader.Read())
                    {
                        dashboard.DetalleVentas.Add(new DashboardRenglonVentaDTO
                        {
                            TipoEntrada = reader["TipoEntrada"].ToString() ?? string.Empty,
                            Precio = reader["Precio"] != DBNull.Value ? Convert.ToDecimal(reader["Precio"]) : 0,
                            CantidadVendida = reader["CantidadVendida"] != DBNull.Value ? Convert.ToInt32(reader["CantidadVendida"]) : 0,
                            Subtotal = reader["Subtotal"] != DBNull.Value ? Convert.ToDecimal(reader["Subtotal"]) : 0
                        });
                    }

                    // Lista de Participantes (Individual o por Equipos)
                    reader.NextResult();
                    var participantesRaw = new List<dynamic>();
                    while (reader.Read())
                    {
                        var row = new ExpandoObject() as IDictionary<string, object?>;
                        for (int i = 0; i < reader.FieldCount; i++)
                            row.Add(reader.GetName(i), reader.IsDBNull(i) ? null : reader[i]);
                        participantesRaw.Add(row);
                    }

                    if (dashboard.TipoCompetencia == "Por Equipos")
                    {
                        if (participantesRaw.Any())
                        {
                            var equiposAgrupados = participantesRaw.GroupBy(p => (int)p.EquipoID);
                            foreach (var grupo in equiposAgrupados)
                            {
                                var primerMiembro = grupo.First();
                                var equipoDto = new DashboardEquipoDTO
                                {
                                    EquipoID = (int)primerMiembro.EquipoID,
                                    NombreEquipo = (string)primerMiembro.NombreEquipo,
                                    NombreLider = (string)primerMiembro.NombreLider
                                };
                                foreach (var miembro in grupo)
                                {
                                    equipoDto.Miembros.Add(new DashboardParticipanteDTO
                                    {
                                        NombreParticipante = (string)miembro.NombreMiembro,
                                        Email = (string)miembro.EmailMiembro,
                                        TipoEntrada = (string)miembro.TipoEntrada,
                                        CodigoCheckIn = (string)miembro.CodigoCheckIn,
                                        FechaCheckIn = (DateTime?)miembro.FechaCheckIn
                                    });
                                }
                                dashboard.Equipos.Add(equipoDto);
                            }
                        }
                    }
                    else // Competencia Individual
                    {
                        foreach (var participante in participantesRaw)
                        {
                            dashboard.ParticipantesIndividuales.Add(new DashboardParticipanteDTO
                            {
                                NombreParticipante = (string)participante.NombreParticipante,
                                Email = (string)participante.Email,
                                TipoEntrada = (string)participante.TipoEntrada,
                                CodigoCheckIn = (string)participante.CodigoCheckIn,
                                FechaCheckIn = (DateTime?)participante.FechaCheckIn
                            });
                        }
                    }
                }
            }
            return dashboard;
        }
    }
}
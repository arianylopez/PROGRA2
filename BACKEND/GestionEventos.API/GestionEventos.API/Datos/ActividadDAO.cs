using GestionEventos.API.Modelos.DTOs;
using Microsoft.Data.SqlClient;
using System.Text;

namespace GestionEventos.API.Datos
{
    public class ActividadDAO
    {
        private readonly string _cadenaSQL;
        public ActividadDAO(string cadenaSQL) 
        { 
            _cadenaSQL = cadenaSQL; 
        }

        public List<ActividadBusquedaDTO> BuscarActividades(string? texto, int? categoriaId, string? tipoPrecio)
        {
            var lista = new List<ActividadBusquedaDTO>();
            var sqlBuilder = new StringBuilder();

            sqlBuilder.Append(@"
                SELECT 'Evento' AS TipoActividad, e.EventoID AS ActividadID, e.Titulo, e.FechaEvento AS Fecha, e.Ubicacion, e.ImagenUrl,
                       COALESCE((SELECT MIN(Precio) FROM TiposEntrada te WHERE te.EventoID = e.EventoID), 0) AS Precio
                FROM Eventos e
                WHERE e.Estado = 'Activo' AND e.FechaEvento >= GETDATE()
                UNION ALL
                SELECT 'Competencia' AS TipoActividad, c.CompetenciaID AS ActividadID, c.Titulo, c.FechaFin AS Fecha, NULL AS Ubicacion, c.ImagenUrl,
                       COALESCE((SELECT MIN(Precio) FROM TiposEntradaCompetencia tec WHERE tec.CompetenciaID = c.CompetenciaID), 0) AS Precio
                FROM Competencias c
                WHERE c.Estado = 'Activo' AND c.FechaFin >= GETDATE()
            ");

            var whereClauses = new List<string>();
            var parameters = new List<SqlParameter>();

            if (!string.IsNullOrWhiteSpace(texto))
            {
                whereClauses.Add("sub.Titulo LIKE @texto");
                parameters.Add(new SqlParameter("@texto", $"%{texto}%"));
            }
            if (categoriaId.HasValue && categoriaId.Value > 0)
            {
                whereClauses.Add(@"EXISTS (
                    SELECT 1 FROM EventoCategorias ec WHERE ec.EventoID = sub.ActividadID AND ec.CategoriaID = @categoriaId AND sub.TipoActividad = 'Evento'
                    UNION ALL
                    SELECT 1 FROM CompetenciaCategorias cc WHERE cc.CompetenciaID = sub.ActividadID AND cc.CategoriaID = @categoriaId AND sub.TipoActividad = 'Competencia'
                )");
                parameters.Add(new SqlParameter("@categoriaId", categoriaId.Value));
            }

            if (!string.IsNullOrEmpty(tipoPrecio))
            {
                if (tipoPrecio.ToLower() == "gratis") whereClauses.Add("sub.Precio = 0");
                if (tipoPrecio.ToLower() == "pago") whereClauses.Add("sub.Precio > 0");
            }

            string finalSql = $"SELECT * FROM ({sqlBuilder.ToString()}) AS sub";
            if (whereClauses.Any())
            {
                finalSql += " WHERE " + string.Join(" AND ", whereClauses);
            }
            finalSql += " ORDER BY sub.Fecha ASC";

            using (var conexion = new SqlConnection(_cadenaSQL))
            {
                conexion.Open();
                var cmd = new SqlCommand(finalSql, conexion);
                if (parameters.Any()) cmd.Parameters.AddRange(parameters.ToArray());

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        lista.Add(new ActividadBusquedaDTO
                        {
                            ActividadID = Convert.ToInt32(reader["ActividadID"]),
                            TipoActividad = reader["TipoActividad"].ToString() ?? "",
                            Titulo = reader["Titulo"].ToString() ?? "",
                            Fecha = Convert.ToDateTime(reader["Fecha"]),
                            Ubicacion = reader["Ubicacion"] as string,
                            ImagenUrl = reader["ImagenUrl"] as string,
                            Precio = Convert.ToDecimal(reader["Precio"])
                        });
                    }
                }
            }
            return lista;
        }
    }
}
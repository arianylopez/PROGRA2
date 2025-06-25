using GestionEventos.API.Modelos;
using Microsoft.Data.SqlClient;

namespace GestionEventos.API.Datos
{
    public class EquipoDAO
    {
        private readonly string _cadenaSQL;
        public EquipoDAO(string cadenaSQL) 
        { 
            _cadenaSQL = cadenaSQL; 
        }

        public bool CrearEquipoConInscripciones(Equipo equipo, List<Inscripcion> inscripciones)
        {
            using (var conexion = new SqlConnection(_cadenaSQL))
            {
                conexion.Open();
                var transaccion = conexion.BeginTransaction();
                try
                {
                    var equipoCreado = CrearEquipo(equipo, conexion, transaccion);
                    foreach (var inscripcion in inscripciones)
                    {
                        inscripcion.EquipoID = equipoCreado.EquipoID;
                    }
                    var inscripcionDAO = new InscripcionDAO(_cadenaSQL);
                    inscripcionDAO.CrearInscripciones(inscripciones, conexion, transaccion);

                    var miembros = inscripciones.Select(i => new Usuario { UsuarioID = i.UsuarioID }).ToList();
                    AgregarMiembros(equipoCreado.EquipoID, miembros, conexion, transaccion);

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

        public Equipo CrearEquipo(Equipo equipo, SqlConnection conexion, SqlTransaction transaccion)
        {
            var sqlEquipo = "INSERT INTO Equipos (CompetenciaID, NombreEquipo, LiderUsuarioID, Estado) VALUES (@cId, @nombre, @liderId, @estado); SELECT SCOPE_IDENTITY();";
            using (var cmdEquipo = new SqlCommand(sqlEquipo, conexion, transaccion))
            {
                cmdEquipo.Parameters.AddWithValue("@cId", equipo.CompetenciaID);
                cmdEquipo.Parameters.AddWithValue("@nombre", equipo.NombreEquipo);
                cmdEquipo.Parameters.AddWithValue("@liderId", equipo.LiderUsuarioID);
                cmdEquipo.Parameters.AddWithValue("@estado", equipo.Estado);
                var nuevoId = cmdEquipo.ExecuteScalar();
                equipo.EquipoID = (nuevoId == null || nuevoId == DBNull.Value) ? 0 : Convert.ToInt32(nuevoId);
                if (equipo.EquipoID == 0) throw new Exception("No se pudo crear el registro del equipo.");
            }
            return equipo;
        }

        public void AgregarMiembros(int equipoId, List<Usuario> miembros, SqlConnection conexion, SqlTransaction transaccion)
        {
            foreach (var miembro in miembros)
            {
                var sqlMiembro = "INSERT INTO MiembrosEquipo (EquipoID, UsuarioID) VALUES (@equipoId, @usuarioId)";
                using (var cmdMiembro = new SqlCommand(sqlMiembro, conexion, transaccion))
                {
                    cmdMiembro.Parameters.AddWithValue("@equipoId", equipoId);
                    cmdMiembro.Parameters.AddWithValue("@usuarioId", miembro.UsuarioID);
                    cmdMiembro.ExecuteNonQuery();
                }
            }
        }

        public void ActualizarEstadoEquipo(int equipoId, string nuevoEstado, SqlConnection conexion, SqlTransaction transaccion)
        {
            var sql = "UPDATE Equipos SET Estado = @estado WHERE EquipoID = @equipoId";
            using (var cmd = new SqlCommand(sql, conexion, transaccion))
            {
                cmd.Parameters.AddWithValue("@estado", nuevoEstado);
                cmd.Parameters.AddWithValue("@equipoId", equipoId);
                cmd.ExecuteNonQuery();
            }
        }

        public void ActualizarEstadoEquipoPorInscripciones(List<int> inscripcionIds, string nuevoEstado, SqlConnection conexion, SqlTransaction transaccion)
        {
            if (!inscripcionIds.Any()) return;
            var sql = $@"
                UPDATE eq
                SET eq.Estado = @estado
                FROM Equipos eq
                WHERE eq.EquipoID IN (
                    SELECT DISTINCT i.EquipoID 
                    FROM Inscripciones i 
                    WHERE i.InscripcionID IN ({string.Join(",", inscripcionIds)}) AND i.EquipoID IS NOT NULL
                )";

            using (var cmd = new SqlCommand(sql, conexion, transaccion))
            {
                cmd.Parameters.AddWithValue("@estado", nuevoEstado);
                cmd.ExecuteNonQuery();
            }
        }

        public bool VerificarUsuarioInscrito(int usuarioId, int competenciaId)
        {
            using (var conexion = new SqlConnection(_cadenaSQL))
            {
                conexion.Open();
                var sql = @"SELECT COUNT(1) FROM Inscripciones i WHERE i.UsuarioID = @usuarioId AND i.TipoActividad = 'Competencia' AND i.ActividadID = @competenciaId";
                var cmd = new SqlCommand(sql, conexion);
                cmd.Parameters.AddWithValue("@usuarioId", usuarioId);
                cmd.Parameters.AddWithValue("@competenciaId", competenciaId);
                return (int)cmd.ExecuteScalar() > 0;
            }
        }
    }
}
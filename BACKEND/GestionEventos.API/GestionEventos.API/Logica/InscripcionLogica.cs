using GestionEventos.API.Datos;
using GestionEventos.API.Modelos;
using GestionEventos.API.Modelos.DTOs;
using Microsoft.Data.SqlClient;
using System.Linq;

namespace GestionEventos.API.Logica
{
    public class InscripcionLogica
    {
        private readonly InscripcionDAO _inscripcionDAO;
        private readonly EventoDAO _eventoDAO;
        private readonly CompetenciaDAO _competenciaDAO;
        private readonly UsuarioDAO _usuarioDAO;
        private readonly EquipoDAO _equipoDAO;
        private readonly PagoDAO _pagoDAO;
        private readonly string _cadenaSQL;

        public InscripcionLogica(IConfiguration configuration)
        {
            var cadena = configuration.GetConnectionString("SqlConnection") ?? "";
            _cadenaSQL = cadena;
            _inscripcionDAO = new InscripcionDAO(cadena);
            _eventoDAO = new EventoDAO(cadena);
            _competenciaDAO = new CompetenciaDAO(cadena);
            _usuarioDAO = new UsuarioDAO(cadena);
            _equipoDAO = new EquipoDAO(cadena);
            _pagoDAO = new PagoDAO(cadena);
        }

        public (bool Exito, string Mensaje, object? Resultado) ProcesarInscripcion(InscripcionRequest request)
        {
            if (string.Equals(request.TipoActividad, "evento", StringComparison.OrdinalIgnoreCase))
            {
                return ProcesarInscripcionEvento(request);
            }
            else if (string.Equals(request.TipoActividad, "competencia", StringComparison.OrdinalIgnoreCase))
            {
                return ProcesarInscripcionCompetencia(request);
            }
            return (false, "Tipo de actividad no válido.", null);
        }

        private (bool, string, object?) ProcesarInscripcionEvento(InscripcionRequest request)
        {
            var entrada = _eventoDAO.ObtenerTipoEntradaPorId(request.TipoEntradaID);

            if (entrada == null) return (false, "El tipo de entrada no es válido.", null);
            if (request.Cantidad <= 0) return (false, "La cantidad debe ser al menos 1.", null);
            if (entrada.CantidadDisponible < request.Cantidad) return (false, $"No hay suficientes entradas disponibles. Solo quedan {entrada.CantidadDisponible}.", null);
            if (DateTime.Now > entrada.FechaFinVenta) return (false, "La venta para este tipo de entrada ha finalizado.", null);

            if (_inscripcionDAO.VerificarInscripcionExistente(request.UsuarioID, "Evento", request.ActividadID))
                return (false, "Ya te encuentras inscrito en este evento.", null);

            var inscripcionesAPago = new List<Inscripcion>();
            for (int i = 0; i < request.Cantidad; i++)
            {
                inscripcionesAPago.Add(new Inscripcion
                {
                    UsuarioID = request.UsuarioID,
                    TipoActividad = "Evento",
                    ActividadID = request.ActividadID,
                    TipoEntradaID = request.TipoEntradaID,
                    Estado = entrada.Precio > 0 ? "Reservado" : "Confirmado"
                });
            }

            if (entrada.Precio > 0)
            {
                decimal montoTotal = entrada.Precio * request.Cantidad;
                var (pago, inscripcionesCreadas) = _pagoDAO.CrearPagoPendiente(request.UsuarioID, montoTotal, inscripcionesAPago);
                if (pago == null) return (false, "Error al iniciar el proceso de pago.", null);

                var respuestaPago = new IniciarPagoResponse { PagoID = pago.PagoID, MontoTotal = pago.MontoTotal, InscripcionIDs = inscripcionesCreadas.Select(i => i.InscripcionID).ToList() };
                return (true, "Pago requerido.", respuestaPago);
            }
            else
            {
                var exito = _inscripcionDAO.InscribirGratis(inscripcionesAPago);
                return exito ? (true, "Inscripción gratuita completada.", null) : (false, "Error al registrar inscripción gratuita.", null);
            }
        }

        private (bool, string, object?) ProcesarInscripcionCompetencia(InscripcionRequest request)
        {
            var competencia = _competenciaDAO.ObtenerCompetenciaPorId(request.ActividadID);
            if (competencia == null || DateTime.Now > competencia.FechaFin) return (false, "La competencia no existe o ha finalizado.", null);

            var tipoEntrada = competencia.Entradas.FirstOrDefault(e => e.TipoEntradaID == request.TipoEntradaID);
            if (tipoEntrada == null) return (false, "El tipo de inscripción no es válido.", null);
            if (DateTime.Now < tipoEntrada.FechaInicioVenta || DateTime.Now > tipoEntrada.FechaFinVenta)
                return (false, "Este tipo de inscripción no está actualmente a la venta.", null);

            int cantidadRequerida = request.EsInscripcionDeEquipo ? (request.EmailsMiembros?.Count ?? 0) + 1 : 1;
            if (tipoEntrada.CantidadDisponible < cantidadRequerida)
                return (false, "No hay suficientes cupos disponibles para este tipo de inscripción.", null);

            if (request.EsInscripcionDeEquipo)
            {
                if (competencia.Tipo != "Por Equipos") return (false, "Esta no es una competencia por equipos.", null);
                if (string.IsNullOrWhiteSpace(request.NombreEquipo)) return (false, "El nombre del equipo es obligatorio.", null);

                var lider = _usuarioDAO.ObtenerUsuarioPorId(request.UsuarioID);
                if (lider == null) return (false, "El usuario líder no existe.", null);

                var miembrosPorEmail = _usuarioDAO.ObtenerUsuariosPorEmails(request.EmailsMiembros ?? new List<string>());
                if (miembrosPorEmail.Count != (request.EmailsMiembros ?? new List<string>()).Count) return (false, "Uno o más correos de miembros no están registrados.", null);

                var todosLosMiembros = new List<Usuario>(lider != null ? new List<Usuario> { lider } : new List<Usuario>()).Concat(miembrosPorEmail).ToList();
                var miembrosUnicos = todosLosMiembros.DistinctBy(u => u.UsuarioID).ToList();

                if (miembrosUnicos.Count < competencia.TamanoMinEquipo || miembrosUnicos.Count > competencia.TamanoMaxEquipo) return (false, $"El equipo debe tener entre {competencia.TamanoMinEquipo} y {competencia.TamanoMaxEquipo} miembros.", null);

                foreach (var miembro in miembrosUnicos)
                {
                    if (_equipoDAO.VerificarUsuarioInscrito(miembro.UsuarioID, request.ActividadID)) return (false, $"El usuario '{miembro.Email}' ya está inscrito en esta competencia.", null);
                }

                var equipo = new Equipo { CompetenciaID = competencia.CompetenciaID, NombreEquipo = request.NombreEquipo!, LiderUsuarioID = request.UsuarioID };
                var inscripciones = miembrosUnicos.Select(m => new Inscripcion { UsuarioID = m.UsuarioID, TipoActividad = "Competencia", ActividadID = competencia.CompetenciaID, TipoEntradaID = tipoEntrada.TipoEntradaID }).ToList();

                if (tipoEntrada.Precio > 0)
                {
                    decimal montoTotal = tipoEntrada.Precio * miembrosUnicos.Count;
                    equipo.Estado = "Reservado";
                    inscripciones.ForEach(i => i.Estado = "Reservado");
                    var (pago, inscripcionesCreadas) = _pagoDAO.CrearPagoPendiente(lider!.UsuarioID, montoTotal, inscripciones, equipo);
                    if (pago == null) return (false, "Error al iniciar el pago del equipo.", null);
                    return (true, "Pago de equipo requerido.", new IniciarPagoResponse { PagoID = pago.PagoID, MontoTotal = pago.MontoTotal, InscripcionIDs = inscripcionesCreadas.Select(i => i.InscripcionID).ToList() });
                }
                else
                {
                    equipo.Estado = "Confirmado";
                    inscripciones.ForEach(i => i.Estado = "Confirmado");
                    var exito = _equipoDAO.CrearEquipoConInscripciones(equipo, inscripciones);
                    return exito ? (true, "Equipo inscrito exitosamente.", null) : (false, "Error al inscribir el equipo.", null);
                }
            }
            else 
            {
                if (_inscripcionDAO.VerificarInscripcionExistente(request.UsuarioID, "Competencia", request.ActividadID)) return (false, "Ya te encuentras inscrito en esta competencia.", null);

                var inscripcion = new Inscripcion { UsuarioID = request.UsuarioID, TipoActividad = "Competencia", ActividadID = request.ActividadID, TipoEntradaID = tipoEntrada.TipoEntradaID, Estado = tipoEntrada.Precio > 0 ? "Reservado" : "Confirmado" };

                if (tipoEntrada.Precio > 0)
                {
                    var (pago, inscripcionesCreadas) = _pagoDAO.CrearPagoPendiente(request.UsuarioID, tipoEntrada.Precio, new List<Inscripcion> { inscripcion });
                    if (pago == null) return (false, "Error al iniciar el pago.", null);
                    return (true, "Pago requerido.", new IniciarPagoResponse { PagoID = pago.PagoID, MontoTotal = pago.MontoTotal, InscripcionIDs = inscripcionesCreadas.Select(i => i.InscripcionID).ToList() });
                }
                else
                {
                    var exito = _inscripcionDAO.InscribirGratis(new List<Inscripcion> { inscripcion });
                    return exito ? (true, "Inscripción individual gratuita completada.", null) : (false, "Error al registrar inscripción.", null);
                }
            }
        }
    }
}
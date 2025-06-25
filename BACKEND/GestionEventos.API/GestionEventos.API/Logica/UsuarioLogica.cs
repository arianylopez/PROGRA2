using GestionEventos.API.Datos;
using GestionEventos.API.Modelos;
using GestionEventos.API.Modelos.DTOs;

namespace GestionEventos.API.Logica
{
    public class UsuarioLogica
    {
        private readonly UsuarioDAO _usuarioDAO;
        public UsuarioLogica(IConfiguration configuration)
        {
            var cadenaConexion = configuration.GetConnectionString("SqlConnection");
            _usuarioDAO = new UsuarioDAO(cadenaConexion ?? "");
        }

        public List<PreguntaSeguridad> ListarPreguntas()
        {
            return _usuarioDAO.ListarPreguntasSeguridad();
        }

        public (bool Exito, string Mensaje, Usuario? Usuario) RegistrarUsuario(Usuario nuevoUsuario)
        {
            if (string.IsNullOrWhiteSpace(nuevoUsuario.Email) || !nuevoUsuario.Email.Contains('@') || !nuevoUsuario.Email.Contains('.'))
                return (false, "El formato del correo electrónico proporcionado no es válido.", null);

            if (string.IsNullOrWhiteSpace(nuevoUsuario.Password) || nuevoUsuario.Password.Length < 6)
                return (false, "La contraseña debe tener al menos 6 caracteres.", null);

            try
            {
                var (exito, mensaje, usuarioRegistrado) = _usuarioDAO.RegistrarUsuario(nuevoUsuario);

                if (exito && usuarioRegistrado != null)
                {
                    usuarioRegistrado.Password = "";
                }
                return (exito, mensaje, usuarioRegistrado);
            }
            catch (Exception ex)
            {
                return (false, ex.Message, null);
            }
        }

        public Usuario? IniciarSesion(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password)) return null;
            var usuario = _usuarioDAO.ObtenerPorEmail(email);
            if (usuario == null || usuario.Password != password) return null;
            return usuario;
        }

        public SolicitarRecuperacionResponse? SolicitarRecuperacion(string email)
        {
            var usuario = _usuarioDAO.ObtenerPorEmail(email);
            if (usuario == null) return null;

            var pregunta = _usuarioDAO.ListarPreguntasSeguridad().FirstOrDefault(p => p.PreguntaID == usuario.PreguntaID);
            if (pregunta == null) return null;

            return new SolicitarRecuperacionResponse
            {
                UsuarioID = usuario.UsuarioID,
                TextoPregunta = pregunta.TextoPregunta
            };
        }

        public bool ConfirmarRecuperacion(ConfirmarRecuperacionRequest request)
        {
            var usuario = _usuarioDAO.ObtenerUsuarioPorId(request.UsuarioID); 
            if (usuario == null) return false;

            if (usuario.RespuestaSeguridad != request.RespuestaSeguridad) return false;

            return _usuarioDAO.ActualizarPassword(request.UsuarioID, request.NuevaPassword);
        }
    }
}
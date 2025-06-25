using GestionEventos.API.Datos;
using GestionEventos.API.Modelos.DTOs;

namespace GestionEventos.API.Logica
{
    public class PerfilLogica
    {
        private readonly InscripcionDAO _inscripcionDAO;
        private readonly FavoritosDAO _favoritosDAO;
        private readonly UsuarioDAO _usuarioDAO;

        public PerfilLogica(IConfiguration config)
        {
            var cadena = config.GetConnectionString("SqlConnection") ?? "";
            _inscripcionDAO = new InscripcionDAO(cadena);
            _favoritosDAO = new FavoritosDAO(cadena);
            _usuarioDAO = new UsuarioDAO(cadena);

        }

        public PerfilInscripcionesDTO ObtenerMisInscripciones(int usuarioId)
        {
            return _inscripcionDAO.ListarMisInscripciones(usuarioId);
        }

        public (bool Exito, string Mensaje) CambiarPassword(CambiarPasswordRequest request)
        {
            if (request == null) return (false, "La petición es inválida.");

            var usuario = _usuarioDAO.ObtenerUsuarioPorId(request.UsuarioID);
            if (usuario == null) return (false, "Usuario no encontrado.");

            if (usuario.Password != request.PasswordActual)
                return (false, "La contraseña actual es incorrecta.");

            if (string.IsNullOrWhiteSpace(request.PasswordNueva) || request.PasswordNueva.Length < 6)
                return (false, "La nueva contraseña debe tener al menos 6 caracteres.");

            if (request.PasswordActual == request.PasswordNueva)
                return (false, "La nueva contraseña no puede ser igual a la contraseña actual.");

            var exito = _usuarioDAO.ActualizarPassword(request.UsuarioID, request.PasswordNueva);
            if (!exito) return (false, "Ocurrió un error al actualizar la contraseña en la base de datos.");

            return (true, "Contraseña cambiada exitosamente.");
        }

        public (bool Exito, string Mensaje) ActualizarPerfil(int usuarioId, ActualizarPerfilRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Nombre) || string.IsNullOrWhiteSpace(request.Email) || !request.Email.Contains('@'))
            {
                return (false, "El nombre y un email válido son obligatorios.");
            }

            var usuarioActual = _usuarioDAO.ObtenerUsuarioPorId(usuarioId);
            if (usuarioActual == null)
            {
                return (false, "El usuario no fue encontrado.");
            }

            if (usuarioActual.Password != request.PasswordActual)
            {
                return (false, "La contraseña actual es incorrecta.");
            }

            var exito = _usuarioDAO.ActualizarPerfil(usuarioId, request.Nombre, request.Email);
            if (!exito)
            {
                return (false, "No se pudo actualizar el perfil. El email ya podría estar en uso por otro usuario.");
            }

            return (true, "Perfil actualizado con éxito.");
        }
    }
}
namespace GestionEventos.API.Modelos.DTOs
{
    public class ActualizarPerfilRequest
    {
        public string Nombre { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordActual { get; set; } = string.Empty;
    }
}
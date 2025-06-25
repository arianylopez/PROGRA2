namespace GestionEventos.API.Modelos.DTOs
{
    public class ConfirmarRecuperacionRequest
    {
        public int UsuarioID { get; set; }
        public string RespuestaSeguridad { get; set; } = string.Empty;
        public string NuevaPassword { get; set; } = string.Empty;
    }
}
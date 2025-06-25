namespace GestionEventos.API.Modelos
{
    public class Usuario
    {
        public int UsuarioID { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public int PreguntaID { get; set; }
        public string RespuestaSeguridad { get; set; } = string.Empty;
    }
}
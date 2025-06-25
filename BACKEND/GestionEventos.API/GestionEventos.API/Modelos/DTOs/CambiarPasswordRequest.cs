namespace GestionEventos.API.Modelos.DTOs
{
    public class CambiarPasswordRequest
    {
        public int UsuarioID { get; set; }
        public string PasswordActual { get; set; } = string.Empty;
        public string PasswordNueva { get; set; } = string.Empty;
    }
}
namespace GestionEventos.API.Modelos.DTOs
{
    public class CheckInRequest
    {
        public string TipoActividad { get; set; } = string.Empty; 
        public int ActividadID { get; set; }
        public int OrganizadorID { get; set; }
        public string CodigoCheckIn { get; set; } = string.Empty;
    }
}
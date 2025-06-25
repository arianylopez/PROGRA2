namespace GestionEventos.API.Modelos
{
    public class Evento
    {
        public int EventoID { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public DateTime FechaEvento { get; set; }
        public string Ubicacion { get; set; } = string.Empty;
        public int OrganizadorID { get; set; }
        public string? ImagenUrl { get; set; } 
        public string ModalidadPago { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public int CantidadInscritos { get; set; }
    }   
}
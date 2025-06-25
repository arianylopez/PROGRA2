namespace GestionEventos.API.Modelos.DTOs
{
    public class InscripcionRequest
    {
        public int UsuarioID { get; set; }
        public string TipoActividad { get; set; } = string.Empty;
        public int ActividadID { get; set; }
        public int TipoEntradaID { get; set; }
        public int Cantidad { get; set; } = 1;
        public bool EsInscripcionDeEquipo { get; set; }
        public string? NombreEquipo { get; set; }
        public List<string>? EmailsMiembros { get; set; }
    }
}
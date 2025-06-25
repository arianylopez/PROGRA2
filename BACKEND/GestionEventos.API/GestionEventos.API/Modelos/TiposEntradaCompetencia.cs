namespace GestionEventos.API.Modelos
{
    public class TiposEntradaCompetencia
    {
        public int TipoEntradaID { get; set; }
        public int CompetenciaID { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public decimal Precio { get; set; }
        public int CantidadTotal { get; set; }
        public int CantidadDisponible { get; set; }
        public DateTime FechaInicioVenta { get; set; }
        public DateTime FechaFinVenta { get; set; }
    }
}
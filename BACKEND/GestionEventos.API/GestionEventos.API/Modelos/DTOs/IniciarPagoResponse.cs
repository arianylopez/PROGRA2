namespace GestionEventos.API.Modelos.DTOs
{
    public class IniciarPagoResponse
    {
        public int PagoID { get; set; }
        public decimal MontoTotal { get; set; }
        public List<int> InscripcionIDs { get; set; } = new List<int>(); 
    }
}
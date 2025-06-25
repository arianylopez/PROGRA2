namespace GestionEventos.API.Datos
{
    public class Conexion
    {
        private readonly string cadenaSQL = string.Empty; 

        public Conexion(IConfiguration configuration)
        {
            cadenaSQL = configuration.GetConnectionString("SqlConnection") ?? string.Empty;
        }

        public string obtenercadenaSQL()
        {
            return cadenaSQL;
        }
    }
}
using GestionEventos.API.Datos;
using GestionEventos.API.Modelos.DTOs;

namespace GestionEventos.API.Logica
{
    public class ActividadLogica
    {
        private readonly ActividadDAO _actividadDAO;
        public ActividadLogica(IConfiguration config)
        {
            _actividadDAO = new ActividadDAO(config.GetConnectionString("SqlConnection") ?? "");
        }
        public List<ActividadBusquedaDTO> Buscar(string? texto, int? catId, string? tipoPrecio)
        {
            return _actividadDAO.BuscarActividades(texto, catId, tipoPrecio);
        }
    }
}
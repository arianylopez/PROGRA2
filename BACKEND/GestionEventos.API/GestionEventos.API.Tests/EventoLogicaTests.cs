using Microsoft.Extensions.Configuration;
using GestionEventos.API.Logica;

namespace GestionEventos.API.Tests
{
    public class EventoLogicaTests
    {
        private IConfiguration _configuration;
        private string _connectionString;
        [SetUp]
        public void Setup()
        {
            _connectionString = "Server=LENOVO1023;Database=GestionEventosDB;Trusted_Connection=True;Encrypt=False;TrustServerCertificate=True;";
            var myConfiguration = new Dictionary<string, string?>
            {
                {"ConnectionStrings:SqlConnection", _connectionString}
            };

            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(myConfiguration)
                .Build();
        }

        [Test]
        public void EliminarEvento_ConInscritos()
        {
            var logica = new EventoLogica(_configuration);

            int eventoConInscritosId = 3;
            int suOrganizadorId = 1;

            var resultado = logica.EliminarEvento(eventoConInscritosId, suOrganizadorId);

            Assert.IsTrue(resultado.Exito, "La operación de deshabilitar debería haber sido exitosa.");

            Assert.That(resultado.Mensaje, Is.EqualTo("El evento tenía inscripciones y ha sido deshabilitado. Se ha simulado el reembolso."));
        }
    }
}
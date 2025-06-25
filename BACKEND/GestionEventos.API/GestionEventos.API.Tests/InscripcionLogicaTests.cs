using NUnit.Framework;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using GestionEventos.API.Logica;
using GestionEventos.API.Modelos.DTOs;
using Microsoft.Data.SqlClient;

namespace GestionEventos.API.Tests
{
    public class InscripcionLogicaTests
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
        public void ProcesarInscripcionEvento_ConEntradaAgotada_DebeDevolverFallo()
        {
            var logica = new InscripcionLogica(_configuration);

            var request = new InscripcionRequest
            {
                UsuarioID = 2,
                TipoActividad = "evento",
                ActividadID = 8,
                TipoEntradaID = 7,
                Cantidad = 1
            };

            var resultado = logica.ProcesarInscripcion(request);

            Assert.IsFalse(resultado.Exito, "La operación debería haber fallado, pero fue exitosa.");
            Assert.That(resultado.Mensaje, Is.EqualTo("No hay suficientes entradas disponibles. Solo quedan 0."));
        }
    }
}
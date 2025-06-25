using GestionEventos.API.Logica;
using GestionEventos.API.Modelos;
using Microsoft.Extensions.Configuration;

namespace GestionEventos.API.Tests
{
    public class UsuarioLogicaTests
    {
        private IConfiguration _configuration;

        private const string ConnectionString = "Server=LENOVO1023;Database=GestionEventosDB;Trusted_Connection=True;Encrypt=False;TrustServerCertificate=True;";


        [SetUp]
        public void Setup()
        {
            var myConfiguration = new Dictionary<string, string?>
            {
                {"ConnectionStrings:SqlConnection", ConnectionString}
            };

            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(myConfiguration)
                .Build();
        }

        [Test]
        public void RegistrarUsuario_ConEmailExistente()
        {
            var logica = new UsuarioLogica(_configuration);

            var usuarioConEmailDuplicado = new Usuario
            {
                Nombre = "Usuario de Prueba",
                Email = "cmartinario1@correo.com", 
                Password = "password123",
                PreguntaID = 1,
                RespuestaSeguridad = "test"
            };

            var resultado = logica.RegistrarUsuario(usuarioConEmailDuplicado);

            Assert.IsFalse(resultado.Exito);

            Assert.That(resultado.Mensaje, Is.EqualTo("El correo electrónico ya está registrado."));

            Assert.IsNull(resultado.Usuario);
        }
    }
}
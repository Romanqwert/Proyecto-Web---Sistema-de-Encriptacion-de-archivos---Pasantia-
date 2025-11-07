using EncriptacionApi.Application.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace EncriptacionApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        // GET: api/<ValuesController>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            var service = new EncryptionService();

            // Genera una clave AES válida (Base64)
            using var aes = Aes.Create();
            var key = Convert.ToBase64String(aes.Key);

            // 🔹 Ejemplo de JSON de configuración
            string json = """
                {
                    "database": {
                        "host": "localhost",
                        "user": "admin",
                        "password": "1234"
                    },
                    "logging": {
                        "level": "info",
                        "file": "logs/app.log"
                    },
                    "hola": "mundo"
                }
                """;

            // Llamas directamente al método EncryptJson (hazlo público temporalmente)
            var encryptedJson = typeof(EncryptionService)
                .GetMethod("EncryptConfigFileAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                .Invoke(service, new object[] { json, key, null });

            Console.WriteLine("Resultado encriptado:");
            Console.WriteLine(encryptedJson);

            /*
            string xml = """
                <dataset>
                  <record>
                    <id>1</id>
                    <first_name>Payton</first_name>
                    <last_name>Dmitrievski</last_name>
                    <email>pdmitrievski0@amazon.de</email>
                    <gender>Male</gender>
                    <ip_address>182.22.138.236</ip_address>
                  </record>
                </dataset>
                """;
            */
             /*
            string xml = """
                <configuration>
                    <appSettings>
                        <add key="User" value="admin" />
                        <add key="Password" value="1234" />
                    </appSettings>
                    <connectionStrings>
                        <add name="DefaultConnection" connectionString="Server=localhost;Database=test;User Id=sa;Password=123;" />
                    </connectionStrings>
                </configuration>
                """;

            // 🔹 Llamada al método privado EncryptXml (usando reflexión)
            var encryptedXml = typeof(EncryptionService)
                .GetMethod("EncryptXml", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                .Invoke(service, new object[] { xml, key });

            Console.WriteLine("Resultado encriptado XML:");
            Console.WriteLine(encryptedXml);*/
            return new string[] { "value1", "value2" };
        }

        // GET api/<ValuesController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<ValuesController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<ValuesController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<ValuesController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}

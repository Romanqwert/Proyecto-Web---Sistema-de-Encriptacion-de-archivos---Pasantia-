using EncriptacionApi.Application.DTOs;
using EncriptacionApi.Application.Interfaces;
using System.Security.Cryptography;
using System.Text.Json;

namespace EncriptacionApi.Application.Services
{
    /// <summary>
    /// Implementación del servicio de encriptación usando AES.
    /// </summary>
    public class EncryptionService : IEncryptionService
    {
        /// Encripta un archivo usando el algoritmo AES.
        /// Genera una nueva Clave (Key) y Vector de Inicialización (IV) para cada encriptación.
        public async Task<EncryptedDataDto> EncryptFileAsync(IFormFile file)
        {
            using var aes = Aes.Create();
            aes.GenerateKey();
            aes.GenerateIV();

            using var outputStream = new MemoryStream();
            using var cryptoStream = new CryptoStream(outputStream, aes.CreateEncryptor(), CryptoStreamMode.Write);

            // Copia el contenido del archivo de entrada al stream de encriptación
            await file.CopyToAsync(cryptoStream);
            await cryptoStream.FlushFinalBlockAsync();

            return new EncryptedDataDto
            {
                EncryptedContent = outputStream.ToArray(),
                Key = aes.Key,
                IV = aes.IV
            };
        }

        /// Desencripta un archivo usando AES con la Clave e IV proporcionados.
        public async Task<MemoryStream> DecryptFileAsync(byte[] encryptedData, byte[] key, byte[] iv)
        {
            using var aes = Aes.Create();
            aes.Key = key;
            aes.IV = iv;

            using var inputStream = new MemoryStream(encryptedData);
            using var outputStream = new MemoryStream();
            using var cryptoStream = new CryptoStream(inputStream, aes.CreateDecryptor(), CryptoStreamMode.Read);

            // Copia el contenido desencriptado al stream de salida
            await cryptoStream.CopyToAsync(outputStream);

            // Rebobinamos el stream de salida para que pueda ser leído desde el principio
            outputStream.Position = 0;
            return outputStream;
        }

        public async Task<byte[]> EncryptConfigFileAsync(IFormFile file, string encryptionKey)
        {
            var extension = Path.GetExtension(file.FileName).ToLower();
            using var reader = new StreamReader(file.OpenReadStream());
            var content = await reader.ReadToEndAsync();

            string encryptedContent = extension switch
            {
                ".json" => EncryptJson(content, encryptionKey),
                ".xml" or ".config" => EncryptXml(content, encryptionKey),
                // ".yaml" or ".yml" => EncryptYaml(content, encryptionKey),
                // ".ini" => EncryptIni(content, encryptionKey),
                _ => throw new NotSupportedException($"Formato {extension} no soportado.")
            };

            return System.Text.Encoding.UTF8.GetBytes(encryptedContent);
        }

        // Funciones para encriptar xml o config
        private string EncryptXml(string xml, string key)
        {
            var doc = new System.Xml.XmlDocument();
            doc.LoadXml(xml);

            EncryptXmlNodes(doc.DocumentElement!, key);

            using var stringWriter = new StringWriter();
            using var xmlWriter = new System.Xml.XmlTextWriter(stringWriter)
            {
                Formatting = System.Xml.Formatting.Indented
            };
            doc.WriteTo(xmlWriter);
            xmlWriter.Flush();
            return stringWriter.ToString();
        }

        private void EncryptXmlNodes(System.Xml.XmlNode node, string key)
        {
            // Encripta el valor del nodo si es texto
            if (node.NodeType == System.Xml.XmlNodeType.Text ||
                node.NodeType == System.Xml.XmlNodeType.CDATA)
            {
                node.Value = EncryptString(node.Value ?? string.Empty, key);
                return;
            }

            // Encripta atributos
            if (node.Attributes != null)
            {
                foreach (System.Xml.XmlAttribute attr in node.Attributes)
                {
                    if (attr.Name == "key" || attr.Name == "name") continue;
                    attr.Value = EncryptString(attr.Value, key);
                }
            }

            // Llama recursivamente para los nodos hijos
            foreach (System.Xml.XmlNode child in node.ChildNodes)
            {
                EncryptXmlNodes(child, key);
            }
        }

        // Funciones para encriptar json
        private string EncryptJson(string json, string key)
        {
            var data = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
            var encrypted = EncryptValuesRecursively(data, key);
            return JsonSerializer.Serialize(encrypted, new JsonSerializerOptions { WriteIndented = true });
        }

        private object EncryptValuesRecursively(object value, string key)
        {
            if (value is JsonElement el)
            {
                // Console.WriteLine($"json element: {value.ToString()}");
                return el.ValueKind switch
                {
                    JsonValueKind.String => EncryptString(el.GetString() ?? "", key),
                    JsonValueKind.Object => EncryptValuesRecursively(JsonToDictionary(el), key),
                    JsonValueKind.Array => el.EnumerateArray().Select(v => EncryptValuesRecursively(v, key)).ToList(),
                    _ => value
                };
            }
            if (value is Dictionary<string, object> dict)
            {
                // Console.WriteLine($"dictionary {value.ToString()}");
                return dict.ToDictionary(kvp => kvp.Key, kvp => EncryptValuesRecursively(kvp.Value, key));
            }
            if (value is string st) {
                return EncryptString(st, key);
            }
            return value;
        }

        private string EncryptString(string plainText, string key)
        {
            using var aes = Aes.Create();
            aes.Key = Convert.FromBase64String(key);
            aes.GenerateIV();
            using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            using var ms = new MemoryStream();
            ms.Write(aes.IV, 0, aes.IV.Length);
            using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            {
                var bytes = System.Text.Encoding.UTF8.GetBytes(plainText);
                cs.Write(bytes, 0, bytes.Length);
            }
            return Convert.ToBase64String(ms.ToArray());
        }

        private Dictionary<string, object> JsonToDictionary(JsonElement element)
        {
            var dict = new Dictionary<string, object>();

            foreach (var property in element.EnumerateObject())
            {
                switch (property.Value.ValueKind)
                {
                    case JsonValueKind.Object:
                        dict[property.Name] = JsonToDictionary(property.Value);
                        break;

                    case JsonValueKind.Array:
                        dict[property.Name] = property.Value.EnumerateArray()
                            .Select(item => item.ValueKind == JsonValueKind.Object
                                ? JsonToDictionary(item)
                                : GetPrimitiveValue(item))
                            .ToList();
                        break;

                    default:
                        dict[property.Name] = GetPrimitiveValue(property.Value);
                        break;
                }
            }

            return dict;
        }

        private object GetPrimitiveValue(JsonElement element)
        {
            return element.ValueKind switch
            {
                JsonValueKind.String => element.GetString() ?? string.Empty,
                JsonValueKind.Number => element.TryGetInt64(out var l) ? l :
                                        element.TryGetDouble(out var d) ? d : element.GetRawText(),
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.Null => null!,
                _ => element.GetRawText()
            };
        }

    }
}

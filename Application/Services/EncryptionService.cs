using EncriptacionApi.Application.DTOs;
using EncriptacionApi.Application.Interfaces;
using Org.BouncyCastle.Pqc.Crypto.Lms;
using System.Security.Cryptography;
using System.Text;
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

        public async Task<(byte[] Bytes, string Name)> ProcessFileAsync(IFormFile file, string? encryptionKey, List<string>? encryptTargets)
        {
            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            var fileBytes = memoryStream.ToArray();

            try
            {
                encryptionKey = Convert.ToBase64String(Encoding.UTF8.GetBytes(encryptionKey));
            }
            catch (InvalidOperationException)
            {
                throw new InvalidOperationException("La clave de encriptación proporcionada no es una cadena Base64 válida.");
            }
            catch (ArgumentNullException)
            {
            }

            var key = encryptionKey ?? Environment.GetEnvironmentVariable("ENCRYPTION_KEY");
            if (string.IsNullOrEmpty(key))
                throw new InvalidOperationException("La clave de encriptación no está configurada.");

            byte[] encryptedBytes;

            string extension = Path.GetExtension(file.FileName).ToLower();
            if (extension == ".json" || extension == ".xml" || extension == ".config")
            {
                if (encryptTargets == null)
                {
                    encryptedBytes = await EncryptConfigFileAsync(file, key);
                    return (encryptedBytes, file.FileName);
                }

                encryptedBytes = await EncryptConfigFileWithTargetsAsync(file, key, encryptTargets);
                return (encryptedBytes, file.FileName);
            }

            encryptedBytes = EncryptFileBytes(fileBytes, key);
            return (encryptedBytes, file.FileName);
        }

        private byte[] EncryptFileBytes(byte[] inputBytes, string keyString)
        {
            using var aes = Aes.Create();
            aes.Key = Convert.FromBase64String(keyString);
            aes.GenerateIV();

            using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            using var ms = new MemoryStream();
            ms.Write(aes.IV, 0, aes.IV.Length); // Prepend IV
            using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            {
                cs.Write(inputBytes, 0, inputBytes.Length);
            }

            return ms.ToArray();
        }

        public byte[] DecryptFileBytes(byte[] encryptedBytes, string keyBase64, string fileName)
        {
            using var aes = Aes.Create();
            
            // Derivar una key de 256 bits(32 bytes) desde el password
            byte[] salt = Encoding.UTF8.GetBytes("MiSaltUnico12345"); // Mejor usar un salt aleatorio y guardarlo
            using var deriveBytes = new Rfc2898DeriveBytes(keyBase64, salt, 10000, HashAlgorithmName.SHA256);
            aes.Key = deriveBytes.GetBytes(32); // 32 bytes = 256 bits
            
            string extension = Path.GetExtension(fileName).ToLower();
            if (extension == ".json" || extension == ".xml" || extension == ".config")
            {
                IFormFile file = ByteArrayToFormFile(encryptedBytes, fileName, "application/json");
                var decryptedBytes = DecryptConfigFileAsync(file, keyBase64).Result;
                return decryptedBytes;
            }

            // El IV está al inicio del archivo (16 bytes)
            var iv = new byte[aes.BlockSize / 8];
            Array.Copy(encryptedBytes, 0, iv, 0, iv.Length);

            aes.IV = iv;

            using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            using var ms = new MemoryStream();
            using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Write))
            {
                // Escribir el resto del archivo (después del IV)
                cs.Write(encryptedBytes, iv.Length, encryptedBytes.Length - iv.Length);
            }

            return ms.ToArray();
        }

        private static IFormFile ByteArrayToFormFile(byte[] fileBytes, string fileName, string contentType = "application/octet-stream")
        {
            var stream = new MemoryStream(fileBytes);
            var formFile = new FormFile(stream, 0, fileBytes.Length, "file", fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = contentType
            };

            return formFile;
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
        #region Encriptar XML / CONFIG
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
        #endregion

        #region Encriptar JSON
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
            if (value is string st)
            {
                return EncryptString(st, key);
            }
            return value;
        }

        private string EncryptString(string plainText, string key)
        {
            using var aes = Aes.Create();

            // Derivar una key de 256 bits(32 bytes) desde el password
            byte[] salt = Encoding.UTF8.GetBytes("MiSaltUnico12345"); // Mejor usar un salt aleatorio y guardarlo
            using var deriveBytes = new Rfc2898DeriveBytes(key, salt, 10000, HashAlgorithmName.SHA256);
            aes.Key = deriveBytes.GetBytes(32); // 32 bytes = 256 bits
            
            // aes.Key = Convert.FromBase64String(key);
            
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
        #endregion

        // Desencriptar config file
        public async Task<byte[]> DecryptConfigFileAsync(IFormFile file, string encryptionKey)
        {
            var extension = Path.GetExtension(file.FileName).ToLower();
            using var reader = new StreamReader(file.OpenReadStream());
            var content = await reader.ReadToEndAsync();

            string decryptedContent = extension switch
            {
                ".json" => DecryptJson(content, encryptionKey),
                ".xml" or ".config" => DecryptXml(content, encryptionKey),
                _ => throw new NotSupportedException($"Formato {extension} no soportado.")
            };

            return System.Text.Encoding.UTF8.GetBytes(decryptedContent);
        }

        #region JSON Decryption

        private string DecryptJson(string json, string key)
        {
            var data = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
            var decrypted = DecryptValuesRecursively(data, key);
            return JsonSerializer.Serialize(decrypted, new JsonSerializerOptions { WriteIndented = true });
        }

        private object DecryptValuesRecursively(object value, string key)
        {
            if (value is JsonElement el)
            {
                return el.ValueKind switch
                {
                    JsonValueKind.String => TryDecryptString(el.GetString() ?? "", key),
                    JsonValueKind.Object => DecryptValuesRecursively(JsonToDictionary(el), key),
                    JsonValueKind.Array => el.EnumerateArray().Select(v => DecryptValuesRecursively(v, key)).ToList(),
                    _ => value
                };
            }
            if (value is Dictionary<string, object> dict)
            {
                return dict.ToDictionary(kvp => kvp.Key, kvp => DecryptValuesRecursively(kvp.Value, key));
            }
            if (value is string st)
            {
                return TryDecryptString(st, key);
            }
            return value;
        }

        #endregion

        #region XML / CONFIG Decryption

        private string DecryptXml(string xml, string key)
        {
            var doc = new System.Xml.XmlDocument();
            doc.LoadXml(xml);
            DecryptXmlNodes(doc.DocumentElement!, key);

            using var stringWriter = new StringWriter();
            using var xmlWriter = new System.Xml.XmlTextWriter(stringWriter)
            {
                Formatting = System.Xml.Formatting.Indented
            };
            doc.WriteTo(xmlWriter);
            xmlWriter.Flush();
            return stringWriter.ToString();
        }

        private void DecryptXmlNodes(System.Xml.XmlNode node, string key)
        {
            // Desencripta valor del nodo si es texto
            if (node.NodeType == System.Xml.XmlNodeType.Text ||
                node.NodeType == System.Xml.XmlNodeType.CDATA)
            {
                node.Value = TryDecryptString(node.Value ?? string.Empty, key);
                return;
            }

            // Desencripta atributos (menos los de "key" y "name")
            if (node.Attributes != null)
            {
                foreach (System.Xml.XmlAttribute attr in node.Attributes)
                {
                    if (attr.Name == "key" || attr.Name == "name") continue;
                    attr.Value = TryDecryptString(attr.Value, key);
                }
            }

            // Recursión sobre nodos hijos
            foreach (System.Xml.XmlNode child in node.ChildNodes)
            {
                DecryptXmlNodes(child, key);
            }
        }

        #endregion

        #region Helpers

        private string TryDecryptString(string cipherText, string key)
        {
            try
            {
                return DecryptString(cipherText, key);
            }
            catch
            {
                // Si no se puede desencriptar (no es Base64 válido o no fue encriptado),
                // se devuelve el texto original
                return cipherText;
            }
        }

        private string DecryptString(string cipherText, string key)
        {
            var fullCipher = Convert.FromBase64String(cipherText);
            using var aes = Aes.Create();
            
            // Derivar una key de 256 bits(32 bytes) desde el password
            byte[] salt = Encoding.UTF8.GetBytes("MiSaltUnico12345"); // Mejor usar un salt aleatorio y guardarlo
            using var deriveBytes = new Rfc2898DeriveBytes(key, salt, 10000, HashAlgorithmName.SHA256);
            aes.Key = deriveBytes.GetBytes(32); // 32 bytes = 256 bits

            // aes.Key = Convert.FromBase64String(key);
            // aes.Key = Convert.FromBase64String(key);

            var iv = new byte[aes.BlockSize / 8];
            var cipher = new byte[fullCipher.Length - iv.Length];

            Buffer.BlockCopy(fullCipher, 0, iv, 0, iv.Length);
            Buffer.BlockCopy(fullCipher, iv.Length, cipher, 0, cipher.Length);
            aes.IV = iv;

            using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            using var ms = new MemoryStream(cipher);
            using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
            using var reader = new StreamReader(cs);
            return reader.ReadToEnd();
        }

        #endregion

        #region Encrypt Config with Targets
        public async Task<byte[]> EncryptConfigFileWithTargetsAsync(IFormFile file, string encryptionKey, List<string> encryptTargets)
        {
            var extension = Path.GetExtension(file.FileName).ToLower();
            using var reader = new StreamReader(file.OpenReadStream());
            var content = await reader.ReadToEndAsync();

            string encryptedContent = extension switch
            {
                ".json" => EncryptJsonWithTargets(content, encryptionKey, encryptTargets),
                ".xml" or ".config" => EncryptXmlWithTargets(content, encryptionKey, encryptTargets),
                _ => throw new NotSupportedException($"Formato {extension} no soportado.")
            };

            return System.Text.Encoding.UTF8.GetBytes(encryptedContent);
        }

        // Encriptar JSON solo en las keys especificadas
        private string EncryptJsonWithTargets(string json, string key, List<string> encryptTargets)
        {
            var data = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
            var encrypted = EncryptJsonRecursivelyWithTargets(data, key, encryptTargets);
            return JsonSerializer.Serialize(encrypted, new JsonSerializerOptions { WriteIndented = true });
        }

        private object EncryptJsonRecursivelyWithTargets(object value, string key, List<string> encryptTargets, string currentPath = "")
        {
            if (value is JsonElement el)
            {
                return el.ValueKind switch
                {
                    JsonValueKind.String => ShouldEncryptKey(currentPath, encryptTargets)
                                            ? EncryptString(el.GetString() ?? "", key)
                                            : el.GetString() ?? "",
                    JsonValueKind.Object => EncryptJsonRecursivelyWithTargets(JsonToDictionary(el), key, encryptTargets, currentPath),
                    JsonValueKind.Array => el.EnumerateArray().Select(v => EncryptJsonRecursivelyWithTargets(v, key, encryptTargets, currentPath)).ToList(),
                    _ => value
                };
            }
            if (value is Dictionary<string, object> dict)
            {
                return dict.ToDictionary(
                    kvp => kvp.Key,
                    kvp => {
                        string newPath = string.IsNullOrEmpty(currentPath) ? kvp.Key : $"{currentPath}.{kvp.Key}";
                        return EncryptJsonRecursivelyWithTargets(kvp.Value, key, encryptTargets, newPath);
                    }
                );
            }
            if (value is string st)
            {
                return ShouldEncryptKey(currentPath, encryptTargets) ? EncryptString(st, key) : st;
            }
            return value;
        }

        // Encriptar XML solo en las keys especificadas
        private string EncryptXmlWithTargets(string xml, string key, List<string> encryptTargets)
        {
            var doc = new System.Xml.XmlDocument();
            doc.LoadXml(xml);

            EncryptXmlNodesWithTargets(doc.DocumentElement!, key, encryptTargets);

            using var stringWriter = new StringWriter();
            using var xmlWriter = new System.Xml.XmlTextWriter(stringWriter)
            {
                Formatting = System.Xml.Formatting.Indented
            };
            doc.WriteTo(xmlWriter);
            xmlWriter.Flush();
            return stringWriter.ToString();
        }

        private void EncryptXmlNodesWithTargets(System.Xml.XmlNode node, string key, List<string> encryptTargets, string currentPath = "")
        {
            string nodeName = node.Name;
            string fullPath = string.IsNullOrEmpty(currentPath) ? nodeName : $"{currentPath}.{nodeName}";

            // Encripta el valor del nodo si es texto y está en los targets
            if (node.NodeType == System.Xml.XmlNodeType.Text || node.NodeType == System.Xml.XmlNodeType.CDATA)
            {
                if (ShouldEncryptKey(fullPath, encryptTargets))
                {
                    node.Value = EncryptString(node.Value ?? string.Empty, key);
                }
                return;
            }

            // Encripta atributos si están en los targets
            if (node.Attributes != null)
            {
                foreach (System.Xml.XmlAttribute attr in node.Attributes)
                {
                    if (attr.Name == "key" || attr.Name == "name") continue;

                    string attrPath = $"{nodeName}.{attr.Name}";
                    if (ShouldEncryptKey(attrPath, encryptTargets) || ShouldEncryptKey(attr.Name, encryptTargets))
                    {
                        attr.Value = EncryptString(attr.Value, key);
                    }
                }
            }

            // Llama recursivamente para los nodos hijos
            foreach (System.Xml.XmlNode child in node.ChildNodes)
            {
                EncryptXmlNodesWithTargets(child, key, encryptTargets, fullPath);
            }
        }

        // Helper para verificar si una key debe ser encriptada
        private bool ShouldEncryptKey(string keyPath, List<string> encryptTargets)
        {
            if (encryptTargets == null || encryptTargets.Count == 0)
                return false;

            // Obtener el último segmento del path (ej: "config.database.password" -> "password")
            string lastSegment = keyPath.Split('.').Last();

            // Verificar si el path completo, el último segmento, o alguna parte del path está en la lista
            return encryptTargets.Any(target =>
                keyPath == target ||                    // Match exacto del path completo
                lastSegment == target ||                // Match del nombre de la key
                keyPath.EndsWith("." + target) ||       // Match del final del path
                keyPath.StartsWith(target + ".") ||     // El path es hijo del target (scripts.ng, scripts.start, etc)
                keyPath.Contains("." + target + ".")    // El target está en medio del path
            );
        }
        #endregion
    }
}

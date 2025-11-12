using EncriptacionApi.Application.DTOs;
using EncriptacionApi.Application.Interfaces;
using EncriptacionApi.Application.Services.Encryption;
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
        private readonly JsonEncryptionService jsonEncryptionService = new JsonEncryptionService();
        private readonly XmlEncryptionService xmlEncryptionService = new XmlEncryptionService();

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

        public byte[] DecryptFileBytes(byte[] encryptedBytes, string keyBase64, string fileName)
        {
            string extension = Path.GetExtension(fileName).ToLower();
            if (extension == ".json" || extension == ".xml" || extension == ".config")
            {
                IFormFile file = ByteArrayToFormFile(encryptedBytes, fileName, "application/json");
                var decryptedBytes = DecryptConfigFileAsync(file, keyBase64).Result;
                return decryptedBytes;
            }

            using var aes = Aes.Create();

            // EXTRAER el salt del archivo (primeros 16 bytes)
            byte[] salt = new byte[16];
            Array.Copy(encryptedBytes, 0, salt, 0, salt.Length);

            // Derivar la clave usando el salt extraído
            using var deriveBytes = new Rfc2898DeriveBytes(keyBase64, salt, 10000, HashAlgorithmName.SHA256);
            aes.Key = deriveBytes.GetBytes(32);

            // EXTRAER el IV (siguientes 16 bytes, después del salt)
            var iv = new byte[aes.BlockSize / 8]; // 16 bytes
            Array.Copy(encryptedBytes, 16, iv, 0, iv.Length);
            aes.IV = iv;

            using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            using var ms = new MemoryStream();
            using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Write))
            {
                // Escribir los datos encriptados (después de salt + IV = 32 bytes)
                int dataStart = 32; // 16 bytes salt + 16 bytes IV
                cs.Write(encryptedBytes, dataStart, encryptedBytes.Length - dataStart);
            }

            return ms.ToArray();
        }
        
        private byte[] EncryptFileBytes(byte[] inputBytes, string keyString)
        {
            using var aes = Aes.Create();

            byte[] salt = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            using var deriveBytes = new Rfc2898DeriveBytes(keyString, salt, 10000, HashAlgorithmName.SHA256);
            aes.Key = deriveBytes.GetBytes(32);
            aes.GenerateIV();

            using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            using var ms = new MemoryStream();

            ms.Write(salt, 0, salt.Length); // Prepend salt
            ms.Write(aes.IV, 0, aes.IV.Length); // Prepend IV

            using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            {
                cs.Write(inputBytes, 0, inputBytes.Length);
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

        private async Task<byte[]> EncryptConfigFileAsync(IFormFile file, string encryptionKey)
        {
            var extension = Path.GetExtension(file.FileName).ToLower();
            using var reader = new StreamReader(file.OpenReadStream());
            var content = await reader.ReadToEndAsync();

            string encryptedContent = extension switch
            {
                ".json" => jsonEncryptionService.EncryptJson(content, encryptionKey),
                ".xml" or ".config" => xmlEncryptionService.EncryptXml(content, encryptionKey),
                // ".yaml" or ".yml" => EncryptYaml(content, encryptionKey),
                // ".ini" => EncryptIni(content, encryptionKey),
                _ => throw new NotSupportedException($"Formato {extension} no soportado.")
            };

            return System.Text.Encoding.UTF8.GetBytes(encryptedContent);
        }

        private async Task<byte[]> DecryptConfigFileAsync(IFormFile file, string encryptionKey)
        {
            var extension = Path.GetExtension(file.FileName).ToLower();
            using var reader = new StreamReader(file.OpenReadStream());
            var content = await reader.ReadToEndAsync();

            string decryptedContent = extension switch
            {
                ".json" => jsonEncryptionService.DecryptJson(content, encryptionKey),
                ".xml" or ".config" => xmlEncryptionService.DecryptXml(content, encryptionKey),
                _ => throw new NotSupportedException($"Formato {extension} no soportado.")
            };

            return System.Text.Encoding.UTF8.GetBytes(decryptedContent);
        }

        private async Task<byte[]> EncryptConfigFileWithTargetsAsync(IFormFile file, string encryptionKey, List<string> encryptTargets)
        {
            var extension = Path.GetExtension(file.FileName).ToLower();
            using var reader = new StreamReader(file.OpenReadStream());
            var content = await reader.ReadToEndAsync();

            string encryptedContent = extension switch
            {
                ".json" => jsonEncryptionService.EncryptJsonWithTargets(content, encryptionKey, encryptTargets),
                ".xml" or ".config" => xmlEncryptionService.EncryptXmlWithTargets(content, encryptionKey, encryptTargets),
                _ => throw new NotSupportedException($"Formato {extension} no soportado.")
            };

            return System.Text.Encoding.UTF8.GetBytes(encryptedContent);
        }
    }
}

using EncriptacionApi.Application.DTOs;
using EncriptacionApi.Application.Interfaces;
using System.Security.Cryptography;

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
    }
}

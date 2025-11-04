using EncriptacionApi.Application.DTOs;
using Microsoft.AspNetCore.Http;

namespace EncriptacionApi.Application.Interfaces
{
    /// Define las operaciones para encriptar y desencriptar datos de archivos.
    public interface IEncryptionService
    {
        /// Encripta el contenido de un IFormFile usando AES.
        // <param name="file">El archivo subido.</param>
        // <returns>Un DTO con el contenido cifrado, la clave y el IV.</returns>
        Task<EncryptedDataDto> EncryptFileAsync(IFormFile file);

        /// Desencripta un flujo de datos usando la clave y el IV proporcionados.
        // <param name="encryptedData">Los datos cifrados.</param>
        // <param name="key">La clave de encriptación.</param>
        // <param name="iv">El vector de inicialización.</param>
        // <returns>Un MemoryStream con los datos desencriptados.</returns>
        Task<MemoryStream> DecryptFileAsync(byte[] encryptedData, byte[] key, byte[] iv);

        Task<byte[]> EncryptConfigFileAsync(IFormFile file, string encryptionKey);
        Task<byte[]> DecryptConfigFileAsync(IFormFile file, string encryptionKey);
    }
}

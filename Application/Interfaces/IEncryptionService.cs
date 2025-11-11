using EncriptacionApi.Application.DTOs;

namespace EncriptacionApi.Application.Interfaces
{
    /// Define las operaciones para encriptar y desencriptar datos de archivos.
    public interface IEncryptionService
    {
        Task<(byte[] Bytes, string Name)> ProcessFileAsync(IFormFile file, string? encriptionKey, List<string>? encryptTargets);

        byte[] DecryptFileBytes(byte[] encryptedBytes, string keyBase64, string fileName);
    }
}

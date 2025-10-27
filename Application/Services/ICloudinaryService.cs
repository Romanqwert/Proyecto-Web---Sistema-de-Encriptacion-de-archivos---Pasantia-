using CloudinaryDotNet.Actions;
using CloudinaryDotNet;

namespace EncriptacionApi.Application.Services
{
    public interface ICloudinaryService
    {
        Task<string> UploadFileAsync(byte[] fileBytes, string fileName);
        Task<string> DownloadFileUrlAsync(string publicId);
    }
    public class CloudinaryService : ICloudinaryService
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryService(IConfiguration config)
        {
            var account = new Account(
                config["Cloudinary:CloudName"],
                config["Cloudinary:ApiKey"],
                config["Cloudinary:ApiSecret"]
            );
            _cloudinary = new Cloudinary(account);
        }

        public async Task<string> UploadFileAsync(byte[] fileBytes, string fileName)
        {
            using var stream = new MemoryStream(fileBytes);
            var uploadParams = new RawUploadParams
            {
                File = new FileDescription(fileName, stream),
                Folder = "archivos_encriptados"
            };
            var uploadResult = await _cloudinary.UploadAsync(uploadParams);
            return uploadResult.SecureUrl.ToString(); // URL HTTPS del archivo
        }

        public async Task<string> DownloadFileUrlAsync(string publicId)
        {
            // Retorna la URL pública (no descarga el binario, solo el enlace)
            var resource = await _cloudinary.GetResourceAsync(new GetResourceParams(publicId));
            return resource.SecureUrl;
        }
    }
}

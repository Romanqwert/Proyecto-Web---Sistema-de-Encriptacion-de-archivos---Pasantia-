using CloudinaryDotNet.Actions;
using CloudinaryDotNet;

namespace EncriptacionApi.Application.Services
{
    public interface ICloudinaryService
    {
        Task<string> UploadFileAsync(byte[] fileBytes, string fileName, string folderName);
        Task<string> DownloadFileUrlAsync(string publicId);

        Task<GetResourceResult> GetResourceAsync(string publicId);
    }
    public class CloudinaryService : ICloudinaryService
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryService(IConfiguration config)
        {
            var account = new Account(
                Environment.GetEnvironmentVariable("CLOUD_NAME"),
                Environment.GetEnvironmentVariable("API_KEY"),
                Environment.GetEnvironmentVariable("API_SECRET")
            );
            _cloudinary = new Cloudinary(account);
        }

        public async Task<string> UploadFileAsync(byte[] fileBytes, string fileName, string folderName)
        {
            using var stream = new MemoryStream(fileBytes);

            var uploadParams = new RawUploadParams
            {
                File = new FileDescription(fileName, stream),
                Folder = folderName, // aquí indicas la carpeta
                PublicId = Path.GetFileNameWithoutExtension(fileName), // opcional
                Overwrite = true
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);
            return uploadResult.SecureUrl.ToString();
        }

        public async Task<string> DownloadFileUrlAsync(string publicId)
        {
            // Retorna la URL pública (no descarga el binario, solo el enlace)
            var resource = await _cloudinary.GetResourceAsync(new GetResourceParams(publicId));
            return resource.SecureUrl;
        }

        public async Task<GetResourceResult> GetResourceAsync(string publicId)
        {
            return await _cloudinary.GetResourceAsync(publicId);
        }
    }
}

using CloudinaryDotNet.Actions;
using CloudinaryDotNet;

namespace EncriptacionApi.Application.Services
{
    public interface ICloudinaryService
    {
        Task<string> UploadFileAsync(byte[] fileBytes, string fileName, string folderName);
        Task<string> DownloadFileUrlAsync(string publicId);

        Task<GetResourceResult> GetResourceAsync(string publicId);

        Task<bool> DeleteFileAsync(string publicId);

        Task<bool> FileExistsAsync(string publicId);
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

        public async Task<bool> DeleteFileAsync(string publicId)
        {
            try
            {
                var deletionParams = new DeletionParams(publicId)
                {
                    ResourceType = ResourceType.Raw
                };

                var result = await _cloudinary.DestroyAsync(deletionParams);
                return result.Result == "ok" || result.Result == "not found";
            } catch (Exception ex)
            {
                throw new Exception($"Error al eliminar archivo de Cloudinary: {ex.Message}");
            }
        }

        public async Task<bool> FileExistsAsync(string publicId)
        {
            if (string.IsNullOrEmpty(publicId))
                return false;

            try
            {
                // Método 1: Usar GetResource (más preciso pero puede ser más lento)
                var getResourceParams = new GetResourceParams(publicId)
                {
                    ResourceType = ResourceType.Raw
                };

                var result = await _cloudinary.GetResourceAsync(getResourceParams);

                // Si no lanza excepción y tiene información válida, el archivo existe
                return result != null &&
                       !string.IsNullOrEmpty(result.PublicId) &&
                       result.Length > 0;
            }
            catch (Exception ex)
            {
                // Si el error es 404 (Not Found), el archivo no existe
                if (ex.Message.Contains("404") || ex.Message.Contains("Not Found"))
                {
                    return false;
                }

                // Para otros errores, loggear y asumir que no existe por seguridad
                Console.WriteLine($"Error al verificar existencia de archivo en Cloudinary: {ex.Message}");
                return false;
            }
        }
    }
}

using EncriptacionApi.Application.DTOs;
using EncriptacionApi.Application.Interfaces;
using EncriptacionApi.Application.Services;
using EncriptacionApi.Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Security.Cryptography;

namespace EncriptacionApi.Controllers
{
    /// Endpoint para encriptar, desencriptar y gestionar archivos.
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Proteger todos los endpoints de este controlador
    public class ArchivosController : ControllerBase
    {
        private readonly IEncryptionService _encryptionService;
        private readonly IArchivoRepository _archivoRepository;
        private readonly IHistorialService _historialService;
        private readonly ICloudinaryService _cloudinaryService;
        
        public ArchivosController(
            IEncryptionService encryptionService,
            IArchivoRepository archivoRepository,
            IHistorialService historialService,
            ICloudinaryService cloudinaryService)
        {
            _encryptionService = encryptionService;
            _archivoRepository = archivoRepository;
            _historialService = historialService;
            _cloudinaryService = cloudinaryService;
        }

        /// Sube, encripta y guarda un archivo.
        [HttpPost("upload")]
        [RequestSizeLimit(10_000_000)] // Límite de 100 MB (ajustar según sea necesario)
        [ProducesResponseType(typeof(ArchivoInfoDto), 200)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No se ha seleccionado ningún archivo.");

            var idUsuario = GetCurrentUserId();
            var ip = GetCurrentIpAddress();

            try
            {
                var (Bytes, Name) = await ProcessFileAsync(file);

                var folderName = $"archivos_usuario_{idUsuario}";

                var fileUrl = await _cloudinaryService.UploadFileAsync(Bytes, Name, folderName);

                var archivo = await SaveFileRecordAsync(file, fileUrl, idUsuario);

                await _historialService.RegistrarAccion(idUsuario, 1, "UPLOAD_FILE", "SUCCESS", ip);

                return Ok(new
                {
                    archivo.IdArchivo,
                    archivo.NombreArchivo
                });
            }
            catch (Exception ex)
            {
                // Loggear ex (no implementado aquí)
                await _historialService.RegistrarAccion(idUsuario, 1, "ENCRYPT_FILE", "FAILURE", ip);
                return StatusCode(500, $"Error interno al encriptar el archivo: {ex.Message}");
            }
        }

        /// Descarga y desencripta un archivo por su ID.
        [HttpGet("download/unencrypted/{id}")]
        [ProducesResponseType(typeof(FileContentResult), 200)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> DownloadUnencrypted(int id)
        {
            var idUsuario = GetCurrentUserId();
            var ip = GetCurrentIpAddress();

            var archivo = await _archivoRepository.GetByIdAsync(id);

            if (archivo == null)
            {
                await _historialService.RegistrarAccion(idUsuario, 2, "DOWNLOAD_FILE", "NOT_FOUND", ip);
                return NotFound("El archivo no existe.");
            }

            if (archivo.IdUsuario != idUsuario)
            {
                await _historialService.RegistrarAccion(idUsuario, 2, "DOWNLOAD_FILE", "FORBIDDEN", ip);
                return Forbid("No tiene permiso para acceder a este archivo.");
            }

            try
            {
                // Extraer publicId desde la URL guardada
                var publicId = ExtraerPublicIdDesdeUrl(archivo.UrlArchivo);

                // Obtener la URL segura del archivo
                var fileUrl = archivo.UrlArchivo;

                // Descargar bytes desde Cloudinary
                using var httpClient = new HttpClient();
                var fileBytes = await httpClient.GetByteArrayAsync(fileUrl);

                await _historialService.RegistrarAccion(idUsuario, 2, "DOWNLOAD_FILE", "SUCCESS", ip);

                return File(fileBytes, archivo.TipoMime, archivo.NombreArchivo);
            }
            catch (Exception ex)
            {
                await _historialService.RegistrarAccion(idUsuario, 2, "DOWNLOAD_FILE", "FAILURE", ip);
                return StatusCode(500, $"Error al descargar el archivo: {ex.Message}");
            }
        }

        /// Descarga y desencripta un archivo por su ID.
        [HttpGet("download/{id}")]
        [ProducesResponseType(typeof(FileContentResult), 200)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Download(int id)
        {
            var idUsuario = GetCurrentUserId();
            var ip = GetCurrentIpAddress();

            var archivo = await _archivoRepository.GetByIdAsync(id);

            if (archivo == null)
            {
                await _historialService.RegistrarAccion(idUsuario, 2, "DOWNLOAD_FILE", "NOT_FOUND", ip);
                return NotFound("El archivo no existe.");
            }

            if (archivo.IdUsuario != idUsuario)
            {
                await _historialService.RegistrarAccion(idUsuario, 2, "DOWNLOAD_FILE", "FORBIDDEN", ip);
                return Forbid("No tiene permiso para acceder a este archivo.");
            }

            try
            {
                // Extraer publicId desde la URL guardada
                var publicId = ExtraerPublicIdDesdeUrl(archivo.UrlArchivo);

                // Obtener la URL segura del archivo
                var fileUrl = archivo.UrlArchivo;

                // Descargar bytes desde Cloudinary
                using var httpClient = new HttpClient();
                var fileBytes = await httpClient.GetByteArrayAsync(fileUrl);

                var keyBase64 = Environment.GetEnvironmentVariable("ENCRYPTION_KEY");
                if (string.IsNullOrEmpty(keyBase64))
                    throw new InvalidOperationException("La clave de encriptación no está configurada.");

                var decryptedBytes = DecryptFileBytes(fileBytes, keyBase64);

                await _historialService.RegistrarAccion(idUsuario, 2, "DOWNLOAD_FILE", "SUCCESS", ip);

                return File(decryptedBytes, archivo.TipoMime, archivo.NombreArchivo);
            }
            catch (Exception ex)
            {
                await _historialService.RegistrarAccion(idUsuario, 2, "DOWNLOAD_FILE", "FAILURE", ip);
                return StatusCode(500, $"Error al descargar el archivo: {ex.Message}");
            }
        }

        /// Obtiene una lista de todos los archivos encriptados por el usuario.
        [HttpGet("list")]
        [ProducesResponseType(typeof(IEnumerable<ArchivoInfoDto>), 200)]
        public async Task<IActionResult> ListFiles()
        {
            var idUsuario = GetCurrentUserId();
            var archivos = await _archivoRepository.FindAsync(a => a.IdUsuario == idUsuario);

            // Mapeamos a DTOs para no enviar el contenido cifrado
            var archivosInfo = archivos.Select(a => new ArchivoInfoDto
            {
                IdArchivo = a.IdArchivo,
                NombreArchivo = a.NombreArchivo,
                TipoMime = a.TipoMime,
                TamanoBytes = a.TamanoBytes,
                FechaSubida = a.FechaSubida,
                UrlArchivo = a.UrlArchivo
            }).OrderByDescending(a => a.FechaSubida);

            return Ok(archivosInfo);
        }

        [HttpGet("list/download")]
        [ProducesResponseType(typeof(IEnumerable<ArchivoInfoDto>), 200)]
        public async Task<IActionResult> ListDownload()
        {
            var idUsuario = GetCurrentUserId();
            var archivos = await _archivoRepository.FindAsync(a => a.IdUsuario == idUsuario);

            var archivosDisponibles = new List<ArchivoInfoDto>();

            foreach (var archivo in archivos)
            {
                int index = archivosDisponibles.FindIndex(a => ExtraerPublicIdDesdeUrl(a.UrlArchivo) == ExtraerPublicIdDesdeUrl(archivo.UrlArchivo));
                if (index == -1)
                {
                    archivosDisponibles.Add(new ArchivoInfoDto
                    {
                        IdArchivo = archivo.IdArchivo,
                        NombreArchivo = archivo.NombreArchivo,
                        TipoMime = archivo.TipoMime,
                        TamanoBytes = archivo.TamanoBytes,
                        FechaSubida = archivo.FechaSubida,
                        UrlArchivo = archivo.UrlArchivo
                    });
                }
            }

            return Ok(archivosDisponibles);
        }

        [HttpGet("history")]
        [ProducesResponseType(typeof(IEnumerable<ArchivoInfoDto>), 200)]
        public async Task<IActionResult> GetHistory()
        {
            var idUsuario = GetCurrentUserId();
            try { 
                var historial = await _historialService.GetUserHistoryAsync(idUsuario);
         
                return Ok(historial);
            } catch (Exception ex)
            {
                return StatusCode(500, $"Error al obtener el historial de acciones. {ex.Message}");
            }
        }
        // --- Métodos de Ayuda ---

        private int GetCurrentUserId()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out var userId))
            {
                throw new InvalidOperationException("No se pudo obtener el ID de usuario desde el token.");
            }
            return userId;
        }

        private string GetCurrentIpAddress()
        {
            return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "desconocida";
        }

        private string ExtraerPublicIdDesdeUrl(string url)
        {
            try
            {
                var uri = new Uri(url);
                var path = uri.AbsolutePath; // /mi_cloud/raw/upload/v17300000/mis_archivos/documento.pdf
                var partes = path.Split("/upload/");

                if (partes.Length < 2)
                    return string.Empty;

                var despuesDeUpload = partes[1];
                var sinVersion = despuesDeUpload.Substring(despuesDeUpload.IndexOf('/') + 1);
                var sinExtension = Path.Combine(Path.GetDirectoryName(sinVersion) ?? "", Path.GetFileNameWithoutExtension(sinVersion));
                return sinExtension.Replace("\\", "/");
            }
            catch
            {
                return string.Empty;
            }
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

        private async Task<(byte[] Bytes, string Name)> ProcessFileAsync(IFormFile file)
        {
            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            var fileBytes = memoryStream.ToArray();

            var key = Environment.GetEnvironmentVariable("ENCRYPTION_KEY");
            if (string.IsNullOrEmpty(key))
                throw new InvalidOperationException("La clave de encriptación no está configurada.");

            byte[] encryptedBytes;

            string extension = Path.GetExtension(file.FileName).ToLower();
            if (extension == ".json" || extension == ".xml" || extension == ".config") { 
                encryptedBytes = await _encryptionService.EncryptConfigFileAsync(file, key);
            }
            else
            {
                encryptedBytes = EncryptFileBytes(fileBytes, key);
            }

            var encryptedFileName = file.FileName;
            return (encryptedBytes, encryptedFileName);
        }

        private async Task<Archivo> SaveFileRecordAsync(IFormFile file, string fileUrl, int idUsuario)
        {
            var archivo = new Archivo
            {
                IdUsuario = idUsuario,
                NombreArchivo = Path.GetFileName(fileUrl),
                TipoMime = file.ContentType,
                TamanoBytes = file.Length,
                FechaSubida = DateTime.UtcNow,
                UrlArchivo = fileUrl
            };

            await _archivoRepository.AddAsync(archivo);
            return archivo;
        }

        private byte[] DecryptFileBytes(byte[] encryptedBytes, string keyBase64)
        {
            using var aes = Aes.Create();
            aes.Key = Convert.FromBase64String(keyBase64);

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
    }
}

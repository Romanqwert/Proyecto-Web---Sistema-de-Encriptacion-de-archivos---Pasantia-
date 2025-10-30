using EncriptacionApi.Application.DTOs;
using EncriptacionApi.Application.Interfaces;
using EncriptacionApi.Application.Services;
using EncriptacionApi.Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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
                Console.WriteLine($"Upload iniciado por usuairo: ${idUsuario}");
                using var memoryStream = new MemoryStream();
                await file.CopyToAsync(memoryStream);
                var fileBytes = memoryStream.ToArray();
                Console.WriteLine("Archivo copiado en memoria");
                // Nombre de la carpeta dinámica
                var folderName = $"archivos_usuario_{idUsuario}";
                Console.WriteLine("Subiendo a Cloudinary...");
                // Subir a Cloudinary dentro de esa carpeta
                var fileUrl = await _cloudinaryService.UploadFileAsync(fileBytes, file.FileName, folderName);
                Console.WriteLine($"Subida Completada: {fileUrl}");
                // Guardar en BD
                var archivo = new Archivo
                {
                    IdUsuario = idUsuario,
                    NombreArchivo = fileUrl,
                    TipoMime = file.ContentType,
                    TamanoBytes = file.Length,
                    FechaSubida = DateTime.UtcNow
                };
                Console.WriteLine("Guardando en BD...");
                await _archivoRepository.AddAsync(archivo);
                Console.WriteLine("Registrando accion...");
                await _historialService.RegistrarAccion(idUsuario, 1, "UPLOAD_FILE", "SUCCESS", ip);

                return Ok(new
                {
                    archivo.IdArchivo,
                    archivo.NombreArchivo
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Upload Error: {ex.Message}");
                // Loggear ex (no implementado aquí)
                await _historialService.RegistrarAccion(idUsuario, 1, "ENCRYPT_FILE", "FAILURE", ip);
                return StatusCode(500, $"Error interno al encriptar el archivo: {ex.Message}");
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
                var publicId = ExtraerPublicIdDesdeUrl(archivo.NombreArchivo);

                // Obtener la URL segura del archivo
                var fileUrl = archivo.NombreArchivo;

                // Descargar bytes desde Cloudinary
                using var httpClient = new HttpClient();
                var fileBytes = await httpClient.GetByteArrayAsync(fileUrl);

                await _historialService.RegistrarAccion(idUsuario, 2, "DOWNLOAD_FILE", "SUCCESS", ip);

                return File(fileBytes, archivo.TipoMime, Path.GetFileName(archivo.NombreArchivo));
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
                FechaSubida = a.FechaSubida
            }).OrderByDescending(a => a.FechaSubida);

            return Ok(archivosInfo);
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

        // https://res.cloudinary.com/dsnwguzkd/raw/upload/v1761568264/archivos_usuario_2/desarrollo_web.txt
        private string ExtraerSubcadena(string url)
        {
            string[] subcadenas = url.Split("/");
            string publicId = subcadenas[6];
            return publicId;
        }
    }
}

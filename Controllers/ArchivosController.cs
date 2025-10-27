using EncriptacionApi.Application.DTOs;
using EncriptacionApi.Application.Interfaces;
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

        public ArchivosController(
            IEncryptionService encryptionService,
            IArchivoRepository archivoRepository,
            IHistorialService historialService)
        {
            _encryptionService = encryptionService;
            _archivoRepository = archivoRepository;
            _historialService = historialService;
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
                // 1. Encriptar el archivo
                var encryptedData = await _encryptionService.EncryptFileAsync(file);

                // 2. Crear la entidad Archivo
                var archivo = new Archivo
                {
                    IdUsuario = idUsuario,
                    NombreArchivo = file.FileName,
                    TipoMime = file.ContentType,
                    TamanoBytes = file.Length,
                    FechaSubida = DateTime.UtcNow,
                    ContenidoCifrado = encryptedData.EncryptedContent,
                    ClaveCifrado = encryptedData.Key,
                    IVCifrado = encryptedData.IV
                };

                // 3. Guardar en la BD
                await _archivoRepository.AddAsync(archivo);

                // 4. Registrar en historial
                await _historialService.RegistrarAccion(idUsuario, null, "ENCRYPT_FILE", "SUCCESS", ip);

                // 5. Devolver DTO con información
                var archivoInfo = new ArchivoInfoDto
                {
                    IdArchivo = archivo.IdArchivo,
                    NombreArchivo = archivo.NombreArchivo,
                    TipoMime = archivo.TipoMime,
                    TamanoBytes = archivo.TamanoBytes,
                    FechaSubida = archivo.FechaSubida
                };

                return Ok(archivoInfo);
            }
            catch (Exception ex)
            {
                // Loggear ex (no implementado aquí)
                await _historialService.RegistrarAccion(idUsuario, null, "ENCRYPT_FILE", "FAILURE", ip);
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

            // Validar que el archivo exista
            if (archivo == null)
            {
                await _historialService.RegistrarAccion(idUsuario, null, "DECRYPT_FILE", "NOT_FOUND", ip);
                return NotFound("El archivo no existe.");
            }

            // Validar que el archivo pertenezca al usuario
            if (archivo.IdUsuario != idUsuario)
            {
                await _historialService.RegistrarAccion(idUsuario, null, "DECRYPT_FILE", "FORBIDDEN", ip);
                return Forbid("No tiene permiso para acceder a este archivo.");
            }

            try
            {
                // 1. Desencriptar el contenido
                var decryptedStream = await _encryptionService.DecryptFileAsync(
                    archivo.ContenidoCifrado,
                    archivo.ClaveCifrado,
                    archivo.IVCifrado);

                // 2. Registrar en historial
                await _historialService.RegistrarAccion(idUsuario, null, "DECRYPT_FILE", "SUCCESS", ip);

                // 3. Devolver el archivo para descarga
                return File(decryptedStream, archivo.TipoMime, archivo.NombreArchivo);
            }
            catch (Exception ex)
            {
                // Loggear ex
                await _historialService.RegistrarAccion(idUsuario, null, "DECRYPT_FILE", "FAILURE", ip);
                return StatusCode(500, $"Error interno al desencriptar el archivo: {ex.Message}");
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
    }
}

using System.ComponentModel.DataAnnotations;

namespace EncriptacionApi.Application.DTOs
{
    public class UploadWithMetadataRequest
    {
        [Required]
        public IFormFile File { get; set; } = null!;

        public string? EncryptionKey { get; set; }

        public List<string>? EncryptTargets { get; set; }
    }
}

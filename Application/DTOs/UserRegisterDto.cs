using System.ComponentModel.DataAnnotations;

namespace EncriptacionApi.Application.DTOs
{
    /// DTO para el registro de un nuevo usuario.
    public class UserRegisterDto
    {
        [Required]
        [StringLength(50)]
        public string NombreUsuario { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(150)]
        public string CorreoElectronico { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; } = string.Empty;
    }
}

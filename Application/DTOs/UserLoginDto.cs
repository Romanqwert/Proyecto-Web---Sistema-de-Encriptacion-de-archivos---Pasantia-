using System.ComponentModel.DataAnnotations;

namespace EncriptacionApi.Application.DTOs
{
    /// DTO para el inicio de sesión de un usuario.
    public class UserLoginDto
    {
        [Required]
        [EmailAddress]
        public string CorreoElectronico { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }
}

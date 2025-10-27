using EncriptacionApi.Core.Entities;

namespace EncriptacionApi.Application.Interfaces
{
    /// Define la operación para generar tokens JWT.
    public interface ITokenService
    {
        /// Genera un token JWT para un usuario específico.
        // <param name="usuario">El usuario para el cual generar el token.</param>
        // <returns>El string del token JWT.</returns>
        string GenerarToken(Usuario usuario);
    }
}

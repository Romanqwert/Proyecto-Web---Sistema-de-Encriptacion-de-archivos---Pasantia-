using EncriptacionApi.Application.Interfaces;
using EncriptacionApi.Core.Entities;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace EncriptacionApi.Application.Services
{
    /// Implementación del servicio de generación de tokens JWT.
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _config;
        private readonly SymmetricSecurityKey _key;

        public TokenService(IConfiguration config)
        {
            _config = config;
            // Obtenemos la clave secreta desde appsettings.json
            var secretKey = _config["Jwt:Key"] ?? throw new InvalidOperationException("La clave JWT (Jwt:Key) no está configurada.");
            _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        }

        /// Genera un token JWT para un usuario.
        public string GenerarToken(Usuario usuario)
        {
            var claims = new List<Claim>
            {
                // Usamos NameIdentifier para el ID, que es más estándar
                new Claim(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString()),
                new Claim(ClaimTypes.Name, usuario.NombreUsuario),
                new Claim(ClaimTypes.Email, usuario.CorreoElectronico)
            };

            var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(2), // Usamos UtcNow
                Issuer = _config["Jwt:Issuer"],
                Audience = _config["Jwt:Audience"],
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}

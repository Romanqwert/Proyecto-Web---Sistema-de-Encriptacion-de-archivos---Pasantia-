using EncriptacionApi.Data;
using EncriptacionApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace EncriptacionApi.Controllers
{
    public class UsuariosController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;

        // Inyección de dependencia del DbContext
        public UsuariosController(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] Usuario user)
        {
            if (await _context.Usuarios.AnyAsync(u => u.NombreUsuario == user.NombreUsuario))
                return BadRequest("El nombre de usuario ya existe.");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
            _context.Usuarios.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Usuario registrado exitosamente."});
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] Usuario credenciales)
        {
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.CorreoElectronico == credenciales.CorreoElectronico);
            if (usuario == null || !BCrypt.Net.BCrypt.Verify(credenciales.PasswordHash, usuario.PasswordHash))
                return Unauthorized("Credenciales inválidas.");

            var token = GenerarToken(usuario);
            return Ok(new { token });
        }

        [Authorize]
        [HttpGet("perfil")]
        public async Task<IActionResult> Perfil()
        {
            var correo = User.FindFirst(ClaimTypes.Email)?.Value;
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.CorreoElectronico == correo);
            if (usuario == null) return NotFound();

            return Ok(new { usuario.IdUsuario, usuario.NombreUsuario, usuario.CorreoElectronico, usuario.FechaRegistro });
        }

        private string GenerarToken(Usuario user)
        {
            var claims = new[] {
                new Claim(ClaimTypes.Name, user.NombreUsuario),
                new Claim(ClaimTypes.Email, user.CorreoElectronico),
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JWT_SECRET"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["JWT_ISSUER"],
                audience: _config["JWT_AUDIENCE"],
                claims: claims,
                expires: DateTime.Now.AddHours(2),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}

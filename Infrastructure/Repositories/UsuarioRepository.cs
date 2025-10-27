using EncriptacionApi.Application.Interfaces;
using EncriptacionApi.Core.Entities;
using EncriptacionApi.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EncriptacionApi.Infrastructure.Repositories
{
    /// Implementación del repositorio de Usuarios.
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly AppDbContext _context;

        public UsuarioRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Usuario usuario)
        {
            await _context.Usuario.AddAsync(usuario);
            await _context.SaveChangesAsync();
        }

        public async Task<Usuario?> GetByEmailAsync(string email)
        {
            return await _context.Usuario
                .FirstOrDefaultAsync(u => u.CorreoElectronico == email);
        }

        public async Task<Usuario?> GetByUsernameAsync(string username)
        {
            return await _context.Usuario
                .FirstOrDefaultAsync(u => u.NombreUsuario == username);
        }

        public async Task<bool> UserExistsAsync(string username)
        {
            return await _context.Usuario.AnyAsync(u => u.NombreUsuario == username);
        }
    }
}

using EncriptacionApi.Application.Interfaces;
using EncriptacionApi.Core.Entities;
using EncriptacionApi.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace EncriptacionApi.Infrastructure.Repositories
{
    /// Implementación del repositorio de Archivos.
    public class ArchivoRepository : IArchivoRepository
    {
        private readonly AppDbContext _context;

        public ArchivoRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Archivo archivo)
        {
            await _context.Archivo.AddAsync(archivo);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Archivo>> FindAsync(Expression<Func<Archivo, bool>> predicate)
        {
            // Usamos AsNoTracking para consultas de solo lectura para mejor rendimiento
            return await _context.Archivo
                .Where(predicate)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Archivo?> GetByIdAsync(int id)
        {
            return await _context.Archivo.FindAsync(id);
        }
    }
}

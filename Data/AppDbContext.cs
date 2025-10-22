using EncriptacionApi.Models;
using Microsoft.EntityFrameworkCore;

namespace EncriptacionApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Usuario> Usuarios { get; set; } = null!;
        public DbSet<Archivo> Archivos { get; set; } = null!;
    }
}

using EncriptacionApi.Models;
using Microsoft.EntityFrameworkCore;

namespace EncriptacionApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {}
        
        public DbSet<Usuario> Usuario { get; set; } = null!;
        public DbSet<Archivo> Archivo { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                // entity.SetTableName(ToSnakeCase(entity.GetTableName()!));

                foreach (var property in entity.GetProperties())
                {
                    property.SetColumnName(ToSnakeCase(property.Name));
                }
            }
        }

        private static string ToSnakeCase(string input)
        {
            return string.Concat(
                input.Select((ch, i) =>
                    i > 0 && char.IsUpper(ch)
                        ? "_" + char.ToLower(ch)
                        : char.ToLower(ch).ToString()
                )
            );
        }
    }
}

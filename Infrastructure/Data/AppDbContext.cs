using EncriptacionApi.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace EncriptacionApi.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // --- DbSets ---
        public DbSet<Usuario> Usuario { get; set; } = null!;
        public DbSet<Archivo> Archivo { get; set; } = null!;
        public DbSet<Historial> Historial { get; set; } = null!;
        public DbSet<Algoritmo> Algoritmo { get; set; } = null!;


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --- Configuración de Relaciones ---

            // Relación Usuario -> Archivos (Uno a Muchos)
            modelBuilder.Entity<Usuario>()
                .HasMany(u => u.Archivos)
                .WithOne(a => a.Usuario)
                .HasForeignKey(a => a.IdUsuario)
                .OnDelete(DeleteBehavior.Cascade); // Si se borra un usuario, se borran sus archivos

            // Relación Usuario -> Historial (Uno a Muchos)
            modelBuilder.Entity<Usuario>()
                .HasMany(u => u.Historiales)
                .WithOne(h => h.Usuario)
                .HasForeignKey(h => h.IdUsuario)
                .OnDelete(DeleteBehavior.Cascade); // Si se borra un usuario, se borra su historial

            // Relación Algoritmo -> Historial (Uno a Muchos)
            modelBuilder.Entity<Algoritmo>()
                .HasMany(a => a.Historiales)
                .WithOne(h => h.Algoritmo)
                .HasForeignKey(h => h.IdAlgoritmo)
                .OnDelete(DeleteBehavior.Restrict); // No permitir borrar un algoritmo si está en uso

            // --- Conversión a Snake Case ---
            // Mantenemos tu lógica de conversión de nombres

            /*
             * ELIMINADO: El siguiente bucle foreach se elimina.
             * CAUSA DEL ERROR: Este bucle sobrescribía la configuración de los atributos [Column("...")]
             * en las entidades. La conversión "ToSnakeCase" en la propiedad "IVCifrado" (C#)
             * resultaba en "i_v_cifrado" (SQL), lo cual es incorrecto.
             * * La forma correcta (que ya está implementada) es usar los atributos [Column("...")]
             * explícitamente en las propiedades de las entidades (como en Archivo.cs).
             */
            // foreach (var entity in modelBuilder.Model.GetEntityTypes())
            // {
            //     foreach (var property in entity.GetProperties())
            //     {
            //         property.SetColumnName(ToSnakeCase(property.Name));
            //     }
            // }
        }

        /// <summary>
        /// Convierte un string de PascalCase a snake_case.
        /// </summary>
        private static string ToSnakeCase(string input)
        {
            if (string.IsNullOrEmpty(input)) { return input; }

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
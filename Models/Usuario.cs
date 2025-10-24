using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EncriptacionApi.Models
{
    public class Usuario
    {
        [Key]
        [Column("id_usuario")]
        public int IdUsuario { get; set; }
        [Column("nombre_usuario")]
        public string NombreUsuario { get; set; } = string.Empty;
        [Column("correo_electronico")]
        public string CorreoElectronico { get; set; } = string.Empty;
        [Column("password_hash")]
        public string PasswordHash { get; set; } = string.Empty;
        [Column("fecha_registro")]
        public DateTime FechaRegistro { get; set; }

        // Relacion con Archivos
        public ICollection<Archivo>? Archivos { get; set; }
    }
}

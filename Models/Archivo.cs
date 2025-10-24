using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EncriptacionApi.Models
{
    public class Archivo
    {
        [Key]
        [Column("id_archivo")]
        public int IdArchivo { get; set; }

        // Relacion con Usuario
        [Column("id_usuario")]
        public int IdUsuario { get; set; }
        public Usuario? Usuario { get; set; }

        [Column("nombre_archivo")]
        public string NombreArchivo { get; set; } = string.Empty;
        [Column("tipo_archivo")]
        public string TipoArchivo { get; set; } = string.Empty;
        [Column("ruta_archivo")]
        public string RutaArchivo { get; set; } = string.Empty;
        [Column("tamaño_kb")]
        public int TamañoKb { get; set; }
        [Column("fecha_subida")]
        public DateTime FechaSubida { get; set; }
    }
}

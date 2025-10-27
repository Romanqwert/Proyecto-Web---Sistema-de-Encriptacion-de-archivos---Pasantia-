using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EncriptacionApi.Core.Entities
{
    /// Representa un archivo encriptado almacenado en la base de datos.
    [Table("Archivo")]
    public class Archivo
    {
        [Key]
        [Column("id_archivo")]
        public int IdArchivo { get; set; }

        [Column("id_usuario")]
        public int IdUsuario { get; set; }

        [Column("nombre_archivo")]
        public string NombreArchivo { get; set; } = string.Empty;

        /// Tipo MIME del archivo original (ej. "application/pdf", "image/jpeg").
        [Column("tipo_mime")]
        public string TipoMime { get; set; } = string.Empty;

        /// Tamaño del archivo original en bytes.
        [Column("tamano_bytes")]
        public long TamanoBytes { get; set; }

        [Column("fecha_subida")]
        public DateTime FechaSubida { get; set; } = DateTime.UtcNow;

        // --- Datos de Encriptación ---

        /// El contenido binario del archivo, encriptado con AES.
        [Column("contenido_cifrado")]
        public byte[] ContenidoCifrado { get; set; } = Array.Empty<byte>();

        /// La clave (Key) de AES usada para encriptar este archivo.
        [Column("clave_cifrado")]
        public byte[] ClaveCifrado { get; set; } = Array.Empty<byte>();

        /// El Vector de Inicialización (IV) de AES usado para encriptar este archivo.
        [Column("iv_cifrado")]
        public byte[] IVCifrado { get; set; } = Array.Empty<byte>();


        // --- Relaciones de Navegación ---

        /// El usuario propietario de este archivo.
        [ForeignKey("IdUsuario")]
        public Usuario? Usuario { get; set; }
    }
}

//public class Archivo
//{
//    [Key]
//    [Column("id_archivo")]
//    public int IdArchivo { get; set; }

//    // Relacion con Usuario
//    [Column("id_usuario")]
//    public int IdUsuario { get; set; }
//    public Usuario? Usuario { get; set; }

//    [Column("nombre_archivo")]
//    public string NombreArchivo { get; set; } = string.Empty;
//    [Column("tipo_archivo")]
//    public string TipoArchivo { get; set; } = string.Empty;
//    [Column("ruta_archivo")]
//    public string RutaArchivo { get; set; } = string.Empty;
//    [Column("tamaño_kb")]
//    public int TamañoKb { get; set; }
//    [Column("fecha_subida")]
//    public DateTime FechaSubida { get; set; }
//}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EncriptacionApi.Core.Entities
{
    /// Registra una acción (ej. encriptar, desencriptar) realizada por un usuario.
    [Table("Historial")]
    public class Historial
    {
        [Key]
        [Column("id_historial")]
        public int IdHistorial { get; set; }

        [Column("id_usuario")]
        public int IdUsuario { get; set; }

        [Column("id_algoritmo")]
        public int IdAlgoritmo { get; set; }

        [Column("accion")]
        public string Accion { get; set; } = string.Empty;

        [Column("fecha_accion")]
        public DateTime FechaAccion { get; set; } = DateTime.UtcNow;

        [Column("resultado")]
        public string Resultado { get; set; } = string.Empty;

        [Column("ip_origen")]
        public string IpOrigen { get; set; } = string.Empty;

        // --- Relaciones de Navegación ---

        [ForeignKey("IdUsuario")]
        public Usuario? Usuario { get; set; }

        [ForeignKey("IdAlgoritmo")]
        public Algoritmo? Algoritmo { get; set; }
    }
}

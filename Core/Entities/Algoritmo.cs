using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EncriptacionApi.Core.Entities
{
    /// Define un algoritmo de encriptación disponible en el sistema.
    [Table("Algoritmo")]
    public class Algoritmo
    {
        [Key]
        [Column("id_algoritmo")]
        public int IdAlgoritmo { get; set; }

        [Column("nombre_algoritmo")]
        public string NombreAlgoritmo { get; set; } = string.Empty;

        [Column("tipo")]
        public string Tipo { get; set; } = string.Empty;

        [Column("longitud_clave")]
        public int? LongitudClave { get; set; }

        // --- Relaciones de Navegación ---
        public ICollection<Historial> Historiales { get; set; } = new List<Historial>();
    }
}

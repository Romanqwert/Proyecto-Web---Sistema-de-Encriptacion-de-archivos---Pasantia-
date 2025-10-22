namespace EncriptacionApi.Models
{
    public class Archivo
    {
        public int IdArchivo { get; set; }

        // Relacion con Usuario
        public int IdUsuario { get; set; }
        public Usuario? Usuario { get; set; }

        public string NombreArchivo { get; set; } = string.Empty;
        public string TipoArchivo { get; set; } = string.Empty;
        public string RutaArchivo { get; set; } = string.Empty;
        public int TamañoKb { get; set; }
        public DateTime FechaSubida { get; set; }
    }
}

namespace EncriptacionApi.Application.DTOs
{
    /// DTO para mostrar información de un archivo (sin su contenido).
    public class ArchivoInfoDto
    {
        public int IdArchivo { get; set; }
        public string NombreArchivo { get; set; } = string.Empty;
        public string TipoMime { get; set; } = string.Empty;
        public long TamanoBytes { get; set; }
        public DateTime FechaSubida { get; set; }
    }
}

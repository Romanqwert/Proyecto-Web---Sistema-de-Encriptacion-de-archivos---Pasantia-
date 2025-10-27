namespace EncriptacionApi.Application.DTOs
{
    /// DTO para transportar los datos de un archivo recién encriptado.
    public class EncryptedDataDto
    {
        public byte[] EncryptedContent { get; set; } = Array.Empty<byte>();
        public byte[] Key { get; set; } = Array.Empty<byte>();
        public byte[] IV { get; set; } = Array.Empty<byte>();
    }
}

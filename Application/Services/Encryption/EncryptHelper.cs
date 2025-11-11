using System.Security.Cryptography;
using System.Text;

namespace EncriptacionApi.Application.Services.Encryption
{
    public class EncryptHelper
    {
        public string EncryptString(string plainText, string key)
        {
            using var aes = Aes.Create();

            // Derivar una key de 256 bits(32 bytes) desde el password
            byte[] salt = Encoding.UTF8.GetBytes("MiSaltUnico12345"); // Mejor usar un salt aleatorio y guardarlo
            using var deriveBytes = new Rfc2898DeriveBytes(key, salt, 10000, HashAlgorithmName.SHA256);
            aes.Key = deriveBytes.GetBytes(32); // 32 bytes = 256 bits

            // aes.Key = Convert.FromBase64String(key);

            aes.GenerateIV();
            using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            using var ms = new MemoryStream();
            ms.Write(aes.IV, 0, aes.IV.Length);
            using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            {
                var bytes = System.Text.Encoding.UTF8.GetBytes(plainText);
                cs.Write(bytes, 0, bytes.Length);
            }
            return Convert.ToBase64String(ms.ToArray());
        }

        public bool ShouldEncryptKey(string keyPath, List<string> encryptTargets)
        {
            if (encryptTargets == null || encryptTargets.Count == 0)
                return false;

            // Obtener el último segmento del path (ej: "config.database.password" -> "password")
            string lastSegment = keyPath.Split('.').Last();

            // Verificar si el path completo, el último segmento, o alguna parte del path está en la lista
            return encryptTargets.Any(target =>
                keyPath == target ||                    // Match exacto del path completo
                lastSegment == target ||                // Match del nombre de la key
                keyPath.EndsWith("." + target) ||       // Match del final del path
                keyPath.StartsWith(target + ".") ||     // El path es hijo del target (scripts.ng, scripts.start, etc)
                keyPath.Contains("." + target + ".")    // El target está en medio del path
            );
        }

        public string TryDecryptString(string cipherText, string key)
        {
            try
            {
                return DecryptString(cipherText, key);
            }
            catch
            {
                // Si no se puede desencriptar (no es Base64 válido o no fue encriptado),
                // se devuelve el texto original
                return cipherText;
            }
        }

        public string DecryptString(string cipherText, string key)
        {
            var fullCipher = Convert.FromBase64String(cipherText);
            using var aes = Aes.Create();

            // Derivar una key de 256 bits(32 bytes) desde el password
            byte[] salt = Encoding.UTF8.GetBytes("MiSaltUnico12345"); // Mejor usar un salt aleatorio y guardarlo
            using var deriveBytes = new Rfc2898DeriveBytes(key, salt, 10000, HashAlgorithmName.SHA256);
            aes.Key = deriveBytes.GetBytes(32); // 32 bytes = 256 bits

            // aes.Key = Convert.FromBase64String(key);
            // aes.Key = Convert.FromBase64String(key);

            var iv = new byte[aes.BlockSize / 8];
            var cipher = new byte[fullCipher.Length - iv.Length];

            Buffer.BlockCopy(fullCipher, 0, iv, 0, iv.Length);
            Buffer.BlockCopy(fullCipher, iv.Length, cipher, 0, cipher.Length);
            aes.IV = iv;

            using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            using var ms = new MemoryStream(cipher);
            using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
            using var reader = new StreamReader(cs);
            return reader.ReadToEnd();
        }
    }
}

using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace MartenFS
{
    public class AesEncryption : IEncryption
    {
        private readonly byte[] _saltBytes;
        private readonly byte[] _passwordBytes;

        public AesEncryption(string password, string salt)
        {
            _passwordBytes = Encoding.ASCII.GetBytes(password);
            _saltBytes = Encoding.ASCII.GetBytes(salt);
        }

        public Stream Encrypt(Stream inStream, Action<Stream> action)
        {
            using (var aes = new AesManaged())
            {
                aes.KeySize = 256;
                aes.BlockSize = 128;

                var key = new Rfc2898DeriveBytes(_passwordBytes, _saltBytes, 88);
                aes.Key = key.GetBytes(aes.KeySize / 8);
                aes.IV = key.GetBytes(aes.BlockSize / 8);

                aes.Mode = CipherMode.CBC;

                // Create a decryptor to perform the stream transform.
                using (ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                using (var crypto = new CryptoStream(inStream, encryptor, CryptoStreamMode.Write))
                {
                    action(crypto);
                    return crypto;
                }
            }
        }

        public Stream Decrypt(Stream inStream, Action<Stream> action)
        {
            using (var aes = new AesManaged())
            {
                aes.KeySize = 256;
                aes.BlockSize = 128;

                var key = new Rfc2898DeriveBytes(_passwordBytes, _saltBytes, 88);
                aes.Key = key.GetBytes(aes.KeySize / 8);
                aes.IV = key.GetBytes(aes.BlockSize / 8);

                aes.Mode = CipherMode.CBC;

                // Create a decryptor to perform the stream transform.
                using (ICryptoTransform encryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                using (var crypto = new CryptoStream(inStream, encryptor, CryptoStreamMode.Write))
                {
                    action(crypto);
                    return crypto;
                }
            }
        }
    }
}
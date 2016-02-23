using System;
using System.IO;
using System.Security.Cryptography;

namespace MartenFS
{
    public interface IEncryption
    {
        Stream Encrypt(Stream inStream, Action<Stream> action);
        Stream Decrypt(Stream inStream, Action<Stream> action);
    }
}
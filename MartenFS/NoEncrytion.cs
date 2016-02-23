using System;
using System.IO;

namespace MartenFS
{
    public class NoEncrytion : IEncryption
    {
        public Stream Encrypt(Stream inStream, Action<Stream> action)
        {
            action(inStream);
            return inStream;
        }

        public Stream Decrypt(Stream inStream, Action<Stream> action)
        {
            action(inStream);
            return inStream;
        }
    }
}
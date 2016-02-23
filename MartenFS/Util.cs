using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MartenFS
{
    public class Util
    {
        public static string NormalizePath(string path, char pathSeparator, bool verifyPath = true)
        {
            var newPath = path.Replace(pathSeparator, '.');

            if (verifyPath && !Regex.IsMatch(newPath, @"^[A-Za-z0-9_\.]+$"))
            {
                throw new Exception("Path not valid. Valid characters are A-Z,a-z,0-9,_,.");
            }

            return newPath;
        }

        public static string DeNormalizePath(string path, char pathSeparator)
        {
            var newPath = path.Replace('.', pathSeparator);
            return newPath;
        }

        public static int CopyToInternal(Stream origin, Stream destination, int bufferSize) //, CancellationToken cancellationToken)
        {
            byte[] buffer = new byte[bufferSize];
            int totalBytes = 0;
            while (true)
            {
                int num = origin.Read(buffer, 0, buffer.Length);
                totalBytes += num;
                int bytesRead;
                if ((bytesRead = num) != 0)
                    destination.Write(buffer, 0, bytesRead);
                else
                    break;
            }
            return totalBytes;
        }

        public static async Task<int> CopyToAsyncInternal(Stream origin, Stream destination, int bufferSize) //, CancellationToken cancellationToken)
        {
            byte[] buffer = new byte[bufferSize];
            int totalBytes = 0;
            while (true)
            {
                int num = await origin.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false);
                totalBytes += num;
                int bytesRead;
                if ((bytesRead = num) != 0)
                    await destination.WriteAsync(buffer, 0, bytesRead).ConfigureAwait(false);
                else
                    break;
            }
            return totalBytes;
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;

namespace MartenFS
{
    public class MartenFile
    {
        private MartenFile()
        {
            Id = Guid.NewGuid();
            Modified = DateTime.UtcNow;
            Created = DateTime.UtcNow;
        }

        public Guid Id { get; internal set; }
        public string Path { get; private set; }
        public string Name { get; private set; }
        public int Size { get; set; }
        public DateTime Modified { get; set; }
        public DateTime Created { get; set; }
        public Dictionary<string, object> Meta { get; set; }
        public int ContentId { get; set; }

        public void SetContentId(uint contentId)
        {
            ContentId = (int) contentId;
        }

        public void SetSize(int size)
        {
            Size = size;
        }

        public static MartenFile FromData(string name, string newPath)
        {
            return new MartenFile()
            {
                Path = newPath,
                Name = name
            };
        }

        public static MartenFile FromData(string name, string newPath, DateTime modified, DateTime created)
        {
            return new MartenFile()
                   {
                       Path = newPath,
                       Name = name,
                       Modified = modified,
                       Created = created
                   };
        }

        public static MartenFile FromFileInfo(string newPath, FileInfo fileInfo)
        {
            return new MartenFile()
                   {
                       Path = newPath,
                       Name = fileInfo.Name,
                       Modified = fileInfo.LastWriteTimeUtc,
                       Created = fileInfo.CreationTimeUtc
                   };
        }
    }
}
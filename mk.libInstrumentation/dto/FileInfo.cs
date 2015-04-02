using System;


namespace mk.libInstrumentation.dto
{
    public class FileInfo
    {
        public string Name { get; set; }
        public string Path { get; set; }

        public string EightDotThreeName { get; set; }

        public string FileName { get; set; }
        public string FileExtension { get; set; }
        public string FileType { get; set; }
        public string Drive { get; set; }

        public long Size { get; set; }

        public long InUseCount { get; set; }

        public DateTime InstallDate { get; set; }
        public DateTime LastAccessed { get; set; }
        public DateTime LastModified { get; set; }

        public string Manufacturer { get; set; }
        public string Version { get; set; }

        public string Caption { get; set; }
        public string Description { get; set; }
    }
}

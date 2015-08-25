using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using APTAT.Valkyrie.Client;

namespace APTAT.Core
{
    public class FileResult
    {
        private  FileResult()
        {
            
        }

        public FileResult(Module badFile)
        {
            Computers = new List<Computer>();
            Id = badFile.Id;
            Path = badFile.Path;
            Cloud = badFile.Cloud;
            Size = badFile.Size;
            Attributes = badFile.Attributes;
            Created = badFile.Created;
            Modified = badFile.Modified;
            Accessed = badFile.Accessed;
            Sha1 = badFile.Sha1;
            Signer = badFile.Signer;
            Description = badFile.Description;
            Version = badFile.Version;
            Copyright = badFile.Copyright;
            Product = badFile.Product;
            
        }
        public string Id { get; set; }
        public string Path { get; set; }
        public CloudVerdict Cloud { get; set; }
        public DateTime CloudDate { get; set; }
        public long Size { get; set; }
        public string Attributes { get; set; }
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }
        public DateTime Accessed { get; set; }
        public string Sha1 { get; set; }
        public string Signer { get; set; }
        public string Description { get; set; }
        public string Version { get; set; }
        public string Copyright { get; set; }
        public string Product { get; set; }

        public string Name 
        {
            get {
                return System.IO.Path.GetFileName(this.Path);
            }
        }
        public FileAttributes FileAttributes
        {
            get
            {
                Int32 flags = 0;
                try
                {
                    flags = Convert.ToInt32(Attributes ?? "0", 16);
                }
                catch (Exception)
                {
                }

                return (FileAttributes)flags;
            }
        }

        public string FileSizeFormatted
        {
            get { return SizeToString(Size); }
        }

        private static string SizeToString(long size)
        {
            if (size < 1024)
                return String.Format("{0} KB ({1} bytes)", 1, size);
            else
                return String.Format("{0} KB ({1} bytes)", (int)(size / (double)1024), size);

        }

        public string FileAge
        {
            get { return FileAgeFormatter.GetAge(Created); }
        }
        
        public AnalysisResult ValkyrieAnalysisResult { get; set; }

        public List<Computer> Computers { get; set; }
    }
}

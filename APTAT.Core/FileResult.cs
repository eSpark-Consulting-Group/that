using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using APTAT.Core.Descent.Apt;
using APTAT.Core.Valkyrie;
using Valkyrie.WebApiProvider.Enums;

namespace APTAT.Core
{
    public class FileResult
    {
        public FileResult(Module badFile)
        {
            Computers = new List<ComputerInformation>();
            Id = badFile.Id;
            Path = badFile.Path;
            Cloud = badFile.Cloud;
            Size = badFile.Size;
            Attributes = badFile.Attributes;
            Created = badFile.Created;
            Modified = badFile.Modified;
            Accessed = badFile.Accessed;
            Sha1 = badFile.Sha1.Trim().ToLower();
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

        public string FullName
        {
            get
            {
                if(String.IsNullOrWhiteSpace(this.Product))
                {
                    return this.Name;
                }

                return String.Format("{0} ({1})", this.Name, this.Product);
            }
        }        

        public bool IsMalware
        {
            get
            {
                if (this.ValkyrieAnalysisResult != null)
                {

                    return ValkyrieAnalysisResult.Verdict == ValkyrieVerdict.Malware || ValkyrieAnalysisResult.Verdict == ValkyrieVerdict.PotentiallyUnwantedApplication;
                }
                return false;
            }
        }

        public string MalwareProbability
        {
            get
            {
                if (this.ValkyrieAnalysisResult != null)
                {
                    return ValkyrieAnalysisResult.MalwareProbability.ToString("P", CultureInfo.InvariantCulture);
                }
                return String.Empty;
            }
        }

        public bool IsUnknown
        {
            get
            {
                if (this.ValkyrieAnalysisResult != null)
                {

                    return ValkyrieAnalysisResult.Verdict == ValkyrieVerdict.Undetected || ValkyrieAnalysisResult.Verdict == ValkyrieVerdict.NotAnalyzed;
                }
                return false;
            }

        }

        public string Name
        {
            get
            {
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

        private static string SizeToString(long len)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            
            int order = 0;
            while (len >= 1024 && order + 1 < sizes.Length)
            {
                order++;
                len = len / 1024;
            }
         
            string result = String.Format("{0:0.##} {1}", len, sizes[order]);

            return result;            
        }

        public string FileAge
        {
            get { return FileAgeFormatter.GetAge(Created); }
        }

        public ValkyrieAnalysisResult ValkyrieAnalysisResult { get; set; }

        public List<ComputerInformation> Computers { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}

using System;
using APTAT.Core.Descent.Apt;

namespace UFH.Application.Models
{
    public class BadFileItem : IBadFileItem
    {
        public virtual Boolean IsUnknown { get { return Cloud == CloudVerdict.Unknown || Cloud == CloudVerdict.FirstSeen; } }

        public virtual Boolean IsMalware { get { return Cloud == CloudVerdict.Malware; } }

        public virtual Boolean IsClean { get { return Cloud == CloudVerdict.Safe; } }

        public String Name { get; set; }

        public String Path { get; set; }

        public Int64 Size { get; set; }

        public String Attributes { get; set; }

        public DateTime Created { get; set; }

        public DateTime Modified { get; set; }

        public DateTime Accessed { get; set; }

        public String Sha1 { get; set; }

        public String Signer { get; set; }

        public String Description { get; set; }

        public String Version { get; set; }

        public String Copyright { get; set; }

        public String Product { get; set; }

        public string FileSizeFormatted
        {
            get { return SizeToString(Size); }
        }

        public string SizeSorting
        {
            get { return SizeToSorting(Size); }
        }

        public CloudVerdict Cloud { get; set; }

        private static string SizeToSorting(long len)
        {
            string[] sizes = { "A", "B", "C", "D" };

            int order = 0;
            while (len >= 1024 && order + 1 < sizes.Length)
            {
                order++;
                len = len / 1024;
            }

            string result = String.Format("{1}{0:0.000}", len / 1000d, sizes[order]);

            return result;
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
    }
}

using System;

namespace APTAT.Application.Models
{
    public class BadFileItem
    {
        public Boolean IsUnknown { get; set; }

        public Boolean IsMalware { get; set; }

        public String Name { get; set; }

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

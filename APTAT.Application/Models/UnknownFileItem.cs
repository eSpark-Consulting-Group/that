using System;

namespace APTAT.Application.Models
{
    public class UnknownFileItem
    {
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
    }
}

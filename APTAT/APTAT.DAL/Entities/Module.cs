using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APTAT.DAL.Entities
{
    public enum CloudVerdict
    {
        Safe, // safe
        Unknown, // unknown
        Malware, // malware
        FirstSeen, // doesntexist
        FailedToAnalyze, // error
        NoVerdict // servererror
    }

    public enum VerdictSource
    {
        RemoteAgent,
        Apt
    }
    public class Module : IEntity
    {
        [NotMapped]
        public object[] EntityKeyValues
        {
            get
            {
                return new object[]
                {
                    this.Id,
                };
            }

            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                if (value.Length != 1)
                    throw new ArgumentOutOfRangeException("value");
                this.Id = (int) value[0];
            }
        }

        [NotMapped]
        public bool EntityKeyValuesAreEmpty
        {
            get
            {
                return (Id == default(int));
            }
        }

        [Key]
        public int Id { get; set; }

        [Required]
        [Index]
        [MaxLength(40)]
        public string Sha1 { get; set; }

        [Required]
        public CloudVerdict CloudVerdict { get; set; }

        [Required]
        public VerdictSource VerdictSource { get; set; }

        [Required]
        public string Path { get; set; }

        [Required]
        [ForeignKey("AptScanResultId")]
        public AptScanResult AptScanResult { get; set; }
        public long AptScanResultId { get; set; }

        public CloudVerdict Cloud { get; set; }
        public DateTime CloudDate { get; set; }
        public long Size { get; set; }
        [MaxLength(6)]
        public string Attributes { get; set; }
        public DateTime CreatedInternal { get; set; }
        public DateTime ModifiedInternal { get; set; }
        public DateTime AccessedInternal { get; set; }
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }
        public DateTime Accessed { get; set; }
        public string Signer { get; set; }
        public string Description { get; set; }
        [MaxLength(300)]
        public string Version { get; set; }
        public string Copyright { get; set; }
        public string Product { get; set; }
        public FileAttributes FileAttributes { get; set; }
        public string FileSizeFormatted { get; set; }
        [MaxLength(20)]
        public string FileAge { get; set; }
        public bool IsBad { get; set; }
        public bool IsUnknownOrFailed { get; set; }
    }
}

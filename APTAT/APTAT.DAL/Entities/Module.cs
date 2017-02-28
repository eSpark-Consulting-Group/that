using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
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
                this.Id = (int)value[0];
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
    }
}

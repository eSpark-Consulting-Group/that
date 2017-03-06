using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace APTAT.DAL.Entities
{
    public class ValkyrieFileInfo : IEntity
    {
        [NotMapped]
        public object[] EntityKeyValues
        {
            get
            {
                return new object[]
                {
                    this.ValkyrieScanInfoId,
                    this.Sha1,
                };
            }

            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                if (value.Length != 2)
                    throw new ArgumentOutOfRangeException("value");
                this.ValkyrieScanInfoId = (int)value[0];
                this.Sha1 = (string)value[1];
            }
        }

        [NotMapped]
        public bool EntityKeyValuesAreEmpty
        {
            get
            {
                return (ValkyrieScanInfoId == default(long) && Sha1 == default(string));
            }
        }

        [Key, Column(Order = 1), ForeignKey(nameof(ValkyrieScanInfo))]
        public int ValkyrieScanInfoId { get; set; }
        [Key, Column(Order = 2)]
        [Required]
        [MaxLength(40)]
        public string Sha1 { get; set; }
        public float? MalwareProbability { get; set; }
        public string OperationStatus { get; set; }
        public string SkipReason { get; set; }
        [Required]
        public ValkyrieVerdict Verdict { get; set; }
        public bool IsWhiteListed { get; set; }
        public virtual ValkyrieScanInfo ValkyrieScanInfo { get; set; }

        public bool IsSubmitted { get; set; }
        public string AnalyzeId { get; set; }
    }

    public enum ValkyrieVerdict
    {
        Clean,

        [Description("No Threat Found")]
        Undetected,

        [Description("In Analysis")]
        FirstSeen,

        Malware,

        [Description("Potentially Unwanted Application")]
        PotentiallyUnwantedApplication,

        [Description("Not Analyzed")]
        NotAnalyzed,

        [Description("In Analysis")]
        Pending,

        [Description("In Analysis")]
        InQueue,

        [Description("Lookup in Progress")]
        PendingFls
    }
}

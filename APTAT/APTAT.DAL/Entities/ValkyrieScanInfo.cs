using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APTAT.DAL.Entities
{
    public class ValkyrieScanInfo : IEntity
    {
        [NotMapped]
        public object[] EntityKeyValues
        {
            get
            {
                return new object[]
                {
                    this.AptScanOperationResultId,
                };
            }

            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                if (value.Length != 1)
                    throw new ArgumentOutOfRangeException("value");
                this.AptScanOperationResultId = (int)value[0];
            }
        }

        [NotMapped]
        public bool EntityKeyValuesAreEmpty
        {
            get
            {
                return (AptScanOperationResultId == default(int));
            }
        }

        public ValkyrieScanInfo()
        {
            ValkyrieFileInfos = new HashSet<ValkyrieFileInfo>();
        }
        
        [StringLength(100)]
        public string ApiKey { get; set; }
        [StringLength(100)]
        public string ScanId { get; set; }

        [Required]
        public DateTime Started { get; set; }
        public DateTime? Finished { get; set; }
        public bool? IsCancelled { get; set; }
        public DateTime? FinishedWithoutErrorHandling { get; set; }
        public string ErrorMessage { get; set; }

        [Required]
        [Key, ForeignKey(nameof(AptScanOperationResult))]
        public int AptScanOperationResultId { get; set; }
        public AptScanOperationResult AptScanOperationResult { get; set; }

        public HashSet<ValkyrieFileInfo> ValkyrieFileInfos { get; set; }
    }
}

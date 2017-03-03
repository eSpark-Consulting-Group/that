using APTAT.DAL.Enums;
using APTAT.DeploymentCore.Directory;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APTAT.DAL.Entities
{
    public class AptScanOperationResult : IEntity
    {
        public AptScanOperationResult()
        {
            AptScanResults = new HashSet<AptScanResult>();
        }
        private System.DateTime _started;
        private System.DateTime? _finished;

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

        public HashSet<AptScanResult> AptScanResults { get; set; }
        //public HashSet<ValkyrieScanInfo> ValkyrieScanInfoes { get; set; }
        public ValkyrieScanInfo ValkyrieScanInfo { get; set; }

        [Key]
        public int Id { get; set; }

        [Required]
        [Index(IsUnique = true)]
        public Guid OperationUid { get; set; }
        
        [Required]
        public System.DateTime Started
        {
            get
            {
                return _started;
            }
            set
            {

                _started = value.Kind == DateTimeKind.Local
                                    ? value.ToUniversalTime()
                                    : DateTime.SpecifyKind(value, DateTimeKind.Utc);
            }
        }
        public System.DateTime? Finished
        {
            get
            {
                return _finished;
            }
            set
            {
                if (value == null) _finished = null;
                else
                    _finished = value.Value.Kind == DateTimeKind.Local
                                    ? value.Value.ToUniversalTime()
                                    : DateTime.SpecifyKind(value.Value, DateTimeKind.Utc);
            }
        }

        public string SourceDescription { get; set; }
        public ETargetsDiscovererType? Source { get; set; }
        public ScanKind ScanMethod { get; set; }
    }
}

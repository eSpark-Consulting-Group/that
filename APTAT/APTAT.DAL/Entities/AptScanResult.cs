using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace APTAT.DAL.Entities
{
    public class AptScanResult : IEntity
    {
        public AptScanResult()
        {
            Modules = new HashSet<Module>();
        }
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
                this.Id = (long)value[0];
            }
        }

        [NotMapped]
        public bool EntityKeyValuesAreEmpty
        {
            get
            {
                return (Id == default(long));
            }
        }
        

        public HashSet<Module> Modules { get; set; }

        [Key]
        public long Id { get; set; }

        [Required]
        [Index]
        public Guid TargetUid { get; set; }

        [Required]
        public int OperationResultId { get; set; }

        [Required]
        [ForeignKey("OperationResultId")]
        public AptScanOperationResult OperationResult { get; set; }

        [Required]
        [StringLength(20)]
        public string IpAddress { get; set; }

        [Required]
        [StringLength(1000)]
        public string ComputerName { get; set; }
        
        [StringLength(50)]
        public string OsName { get; set; }
        public bool Succeeded { get; set; }
        [StringLength(100)]
        public string StatusMessage { get; set; }
        public string OrganizationUnit { get; set; }
        public string LdapPath { get; set; }
        public ScanStage ScanStage { get; set; }
    }
}

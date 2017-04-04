using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace APTAT.DAL.Entities
{
    public class Email : IEntity
    {
        [NotMapped]
        public object[] EntityKeyValues
        {
            get;set;
        }

        [NotMapped]
        public bool EntityKeyValuesAreEmpty
        {
            get;
        }

        [Key]
        public int Id { get; set; }

        [MaxLength(254)]
        public string Address { get; set; }
    }
}

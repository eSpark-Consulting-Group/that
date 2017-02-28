using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APTAT.DAL
{
    public interface IEntity
    {
        object[] EntityKeyValues { get; set; }
        bool EntityKeyValuesAreEmpty { get; }
    }
}

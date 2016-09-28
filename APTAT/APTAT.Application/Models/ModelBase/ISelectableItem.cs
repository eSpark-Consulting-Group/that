using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APTAT.Application.Models.ModelBase
{
    public interface ISelectableItem
    {
        Boolean IsSelected { get; set; }
    }
}

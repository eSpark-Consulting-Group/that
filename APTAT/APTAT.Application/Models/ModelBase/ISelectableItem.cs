using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APTAT.Application.Models.ModelBase
{
    public interface ISelectableItem
    {
        Boolean? IsSelected { get; set; }
    }

    public interface ISelectableTreeItem<T> : ISelectableItem where T : class, ISelectableTreeItem<T>
    {
        ObservableCollection<T> Items { get;  }
    }
}

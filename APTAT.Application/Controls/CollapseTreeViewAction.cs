using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Interactivity;
using Telerik.Windows.Controls;

namespace APTAT.Application.Controls
{
    public class CollapseTreeViewAction : TargetedTriggerAction<RadTreeListView>
    {
        protected override void Invoke(object parameter)
        {
            Target.CollapseAllHierarchyItems();
        }
    }
}

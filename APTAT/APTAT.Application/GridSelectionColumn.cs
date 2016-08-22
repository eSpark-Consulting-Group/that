using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

using System.Windows.Controls.Primitives;
using System.Windows.Data;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.GridView;

namespace APTAT.Application
{
    public class GridSelectionColumn : GridViewColumn
    {
        public GridSelectionColumn()
        {
            IsResizable = false;
            IsReorderable = false;
            Width = GridViewLength.Auto;
            MinWidth = 25;
        }
        public override bool CanFilter()
        {
            return false;
        }
        public override bool CanSort()
        {
            return false;
        }
        public override bool CanGroup()
        {
            return false;
        }
        public override FrameworkElement CreateCellElement(GridViewCell cell, object dataItem)
        {
            if (cell != null)
            {
                System.Windows.Controls.CheckBox checkBox = cell.Content as System.Windows.Controls.CheckBox
                    ?? new System.Windows.Controls.CheckBox
                    {
                        IsTabStop = false,
                    };

                

                //checkBox.SetBinding(UIElement.IsEnabledProperty,
                //    new Binding("IsOnline")
                //    {
                //        Source = dataItem,
                //        Mode = BindingMode.OneWay
                //    });
                

                var row = cell.ParentRow;
                if (row != null)
                {
                    checkBox.HorizontalAlignment = HorizontalAlignment.Center;
                    checkBox.SetBinding(ToggleButton.IsCheckedProperty,
                        new Binding("IsSelected")
                        {
                            Source = row,
                            Mode = BindingMode.TwoWay
                        });
                }
                return checkBox;
            }
            return base.CreateCellElement(cell, dataItem);
        }
    }
}

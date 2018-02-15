using System;
using System.Windows;
using System.Windows.Controls;
using UFH.Application.Models;

namespace UFH.Application.TemplateSelectors
{
    public class ComputerFileTemplateSelector : DataTemplateSelector
    {
        public DataTemplate FileTemplate { get; set; }
        public DataTemplate ComputerTemplate { get; set; }

        public override DataTemplate SelectTemplate(Object item, DependencyObject container)
        {
            if (item is IThreatInfoItem)
            {
                IThreatInfoItem threatInfo = (IThreatInfoItem)item;
                if (threatInfo.IsComputer)
                {
                    return ComputerTemplate;
                }
                return FileTemplate;
            }
            return null;
        }
    }
}

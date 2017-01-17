using System;
using System.Windows;
using System.Windows.Controls;
using FAT.Application.Models;

namespace FAT.Application.TemplateSelectors
{
    public class MainViewNameTemplateSelector : DataTemplateSelector
    {
        public DataTemplate UnknownFileTemplate { get; set; }
        public DataTemplate ComputerTemplate { get; set; }

        public override DataTemplate SelectTemplate(Object item, DependencyObject container)
        {
            if (item is ThreatInfoItem)
            {
                ThreatInfoItem threatInfo = (ThreatInfoItem)item;
                if(threatInfo.IsRootItem)
                {
                    return ComputerTemplate;
                }
                return UnknownFileTemplate;
            }
            return null;
        }
        
    }

  
}

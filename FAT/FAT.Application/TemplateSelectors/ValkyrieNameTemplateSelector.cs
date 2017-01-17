using System;
using System.Windows;
using System.Windows.Controls;
using APTAT.Application.Models.Valkyrie;
using APTAT.Core;

namespace APTAT.Application.TemplateSelectors
{
    class ValkyrieNameTemplateSelector : DataTemplateSelector
    {
        public DataTemplate FileTemplate { get; set; }
        public DataTemplate ComputerTemplate { get; set; }

        public override DataTemplate SelectTemplate(Object item, DependencyObject container)
        {
            if (item is ValkyrieFile)
            {                
                return FileTemplate;
            }
            
            if(item is ValkyrieComputer)
            {
                return ComputerTemplate;
            }
            return null;
        }
        
      
    }
}

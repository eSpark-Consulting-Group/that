using System;
using System.Windows;
using System.Windows.Controls;
using FAT.Application.Models.Valkyrie;
using APTAT.Core;

namespace FAT.Application.TemplateSelectors
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

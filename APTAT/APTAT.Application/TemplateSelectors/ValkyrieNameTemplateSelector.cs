using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using APTAT.Core;

namespace APTAT.Application.TemplateSelectors
{
    class ValkyrieNameTemplateSelector : DataTemplateSelector
    {
        public DataTemplate FileTemplate { get; set; }
        public DataTemplate ComputerTemplate { get; set; }

        public override DataTemplate SelectTemplate(Object item, DependencyObject container)
        {
            if (item is FileResult)
            {                
                return FileTemplate;
            }
            
            if(item is ComputerInformation)
            {
                return ComputerTemplate;
            }
            return null;
        }
        
      
    }
}

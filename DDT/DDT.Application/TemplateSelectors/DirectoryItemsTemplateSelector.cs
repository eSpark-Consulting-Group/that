
using System.Windows;
using System.Windows.Controls;
using DDT.Application.Models;

namespace DDT.Application.TemplateSelectors
{
    public class DirectoryItemsTemplateSelector : DataTemplateSelector
    {
        public DataTemplate RootTemplate { get; set; }
        public DataTemplate ComputerTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item == null)
                return null;

            if ((item as DirectoryItem).Items == null)
                return ComputerTemplate;
            return RootTemplate;
        }
    }
}

using System.Windows;
using System.Windows.Controls;
using DDT.Application.Models;

namespace DDT.Application.TemplateSelectors
{
    public class ResultsTemplateSelector : DataTemplateSelector
    {
        public DataTemplate DeploymentTemplate { get; set; }
        public DataTemplate ComputerTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is DeploymentItem)
                return ComputerTemplate;
            return DeploymentTemplate;
        }
    }
}
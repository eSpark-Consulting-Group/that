using System.Reflection;
using System.Windows;

namespace DDT.Application.Views
{
    public partial class AboutView
    {
        public AboutView()
        {
            InitializeComponent();

            version.Text = $"{Assembly.GetExecutingAssembly().GetName().Version} beta";
        }
    }
}

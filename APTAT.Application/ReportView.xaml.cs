using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Resources;
using System.Windows.Shapes;
using Telerik.Windows.Documents.Fixed;

namespace APTAT.Application
{
    /// <summary>
    /// Interaction logic for ReportView.xaml
    /// </summary>
    public partial class ReportView : UserControl
    {
        public ReportView()
        {
            InitializeComponent();
            Loaded += ReportView_Loaded;
        }

        void ReportView_Loaded(object sender, RoutedEventArgs e)
        {
            StreamResourceInfo stream =
                System.Windows.Application.GetResourceStream(
                    new Uri("pack://application:,,,/kwl.pdf", UriKind.RelativeOrAbsolute));
            pdfViewer.DocumentSource = new PdfDocumentSource(stream.Stream);
        }

        private void tbCurrentPage_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null)
            {
                if (e.Key == System.Windows.Input.Key.Enter)
                {
                    textBox.GetBindingExpression(TextBox.TextProperty).UpdateSource();
                }
            }
        }

        private void tbFind_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                this.pdfViewer.Commands.FindCommand.Execute(this.tbFind.Text);
                this.btnPrev.Visibility = System.Windows.Visibility.Visible;
                this.btnNext.Visibility = System.Windows.Visibility.Visible;
            }
        }
    }
}

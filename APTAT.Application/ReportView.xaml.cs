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
using APTAT.Application.Models;
using Telerik.Windows.Documents.Fixed;
using Telerik.Windows.Documents.Fixed.FormatProviders;

namespace APTAT.Application
{
    /// <summary>
    /// Interaction logic for ReportView.xaml
    /// </summary>
    public partial class ReportView 
    {
        private readonly ReportViewModel _viewModel;

        public ReportView()
        {
            InitializeComponent();
            DataContext = _viewModel = new ReportViewModel();
            _viewModel.ReportOpened += OnReportOpened;

            

            pdfViewer.DefaultFormatProviderSettings = FormatProviderSettings.ReadOnDemand;
        }

        private void OnReportOpened(Object sender, EventArgs e)
        {
            pdfViewer.DocumentSource = new PdfDocumentSource(_viewModel.ReportStream);
        }

        private void TbCurrentPageKeyDown(Object sender, KeyEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null)
            {
                if (e.Key == Key.Enter)
                {
                    textBox.GetBindingExpression(TextBox.TextProperty).UpdateSource();
                }
            }
        }

        private void TbFindKeyDown(Object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                this.pdfViewer.Commands.FindCommand.Execute(this.tbFind.Text);
                this.btnPrev.Visibility = Visibility.Visible;
                this.btnNext.Visibility = Visibility.Visible;
            }
        }
    }
}

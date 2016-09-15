using System;

using System.Diagnostics;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

using System.Windows.Input;


using APTAT.Application.Models;
using APTAT.Common;
using Telerik.Windows.Controls;
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
            DataContext = _viewModel = CompositionHelper.Instance.Resolve<ReportViewModel>();
            _viewModel.ReportOpened += OnReportOpened;

            Unloaded += OnUnloaded;

            pdfViewer.DefaultFormatProviderSettings = FormatProviderSettings.ReadOnDemand;
        }

        private void OnUnloaded(Object sender, RoutedEventArgs e)
        {
            btnPrintPdfDocument.Command = null;
            btnPageDown.Command = null;
            btnPageUp.Command = null;
            btnNext.Command = null;
            btnPrev.Command = null;
            btnZoomIn.Command = null;
            btnZoomOut.Command = null;
            tbFind.Text = null;
            tbPagesCount.Text = null;

            BindingOperations.ClearBinding(tbCurrentPage, TextBox.TextProperty);
            BindingOperations.ClearBinding(cbScale, RadComboBox.TextProperty);
            BindingOperations.ClearBinding(btnTogglePan, RadToggleButton.IsCheckedProperty);
            BindingOperations.ClearBinding(btnToggleTextSelection, RadToggleButton.IsCheckedProperty);

            _viewModel.ReportOpened -= OnReportOpened;
            pdfViewer.DocumentSource = null;
            DataContext = null;
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
            if (e.Key == Key.Enter && (tbFind.Text ?? string.Empty).Length > 2)
            {
                pdfViewer.CommandDescriptors.FindCommandDescriptor.Command.Execute(tbFind.Text);
                btnPrev.Visibility = Visibility.Visible;
                btnNext.Visibility = Visibility.Visible;
            }
        }

        private void OnShowInFolder(Object sender, RoutedEventArgs e)
        {
            Process.Start(@"Explorer.exe", @"/select," + _viewModel.FileName);
        }
    }
}

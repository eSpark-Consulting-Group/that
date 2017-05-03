using System;



using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

using System.Windows.Input;


using FAT.Application.Models;
using APTAT.Common;

using Telerik.Windows.Documents.Fixed;
using Telerik.Windows.Documents.Fixed.FormatProviders;
using Microsoft.Win32;
using System.IO;


namespace FAT.Application
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

        private void pdf_DocumentChanged(object sender, DocumentChangedEventArgs e)
        {
            pdfViewer.ScaleFactor = 1.175d;
        }

        private void OnUnloaded(Object sender, RoutedEventArgs e)
        {
            btnPrintPdfDocument.Command = null;
            btnPageDown.Command = null;
            btnPageUp.Command = null;
            //btnNext.Command = null;
            //btnPrev.Command = null;
            //btnZoomIn.Command = null;
            //btnZoomOut.Command = null;
            //tbFind.Text = null;
            //tbPagesCount.Text = null;

            BindingOperations.ClearBinding(tbCurrentPage, TextBox.TextProperty);
            // BindingOperations.ClearBinding(cbScale, RadComboBox.TextProperty);
            // BindingOperations.ClearBinding(btnTogglePan, RadToggleButton.IsCheckedProperty);
            //BindingOperations.ClearBinding(btnToggleTextSelection, RadToggleButton.IsCheckedProperty);

            _viewModel.ReportOpened -= OnReportOpened;
            pdfViewer.DocumentSource = null;
            DataContext = null;
        }

        private void OnReportOpened(Object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke((Action) delegate
             {
                 using (var stream = new MemoryStream(_viewModel.DocumentBytes))
                 {
                     pdfViewer.DocumentSource = new PdfDocumentSource(stream);
                 }
             });

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

            if (e.Key == Key.Enter && (search.Text ?? string.Empty).Length > 2)
            {
                pdfViewer.CommandDescriptors.FindCommandDescriptor.Command.Execute(search.Text);
                //btnPrev.Visibility = Visibility.Visible;
                //btnNext.Visibility = Visibility.Visible;
            }
        }

        private void TbFindKeyUp(Object sender, KeyEventArgs e)
        {
            if (string.IsNullOrEmpty(search.Text))
                clear.Visibility = Visibility.Collapsed;
            else
                clear.Visibility = Visibility.Visible;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            search.Text = null;
            search.Dispatcher.BeginInvoke((Action) delegate { search.Focus(); });
            clear.Visibility = Visibility.Collapsed;
        }

        private void OnSave(object sender, RoutedEventArgs e)
        {
            var saveDialog = new SaveFileDialog();
            saveDialog.Filter = "PDF File|*.pdf";
            saveDialog.FileName = Path.GetFileName(_viewModel.FileName);

            bool? dialogResult = saveDialog.ShowDialog();
            if (dialogResult == true)
            {
                using (var output = File.Create(saveDialog.FileName))
                using (var input = new MemoryStream(_viewModel.DocumentBytes))
                {
                    input.Seek(0, SeekOrigin.Begin);
                    input.CopyTo(output);
                    output.Flush();
                }

                _viewModel.OnSaveReportCompleted();
            }
        }

        private void OnPrint(object sender, RoutedEventArgs e)
        {
            if (pdfViewer.Settings.PrintSettings != null && pdfViewer.Document != null && pdfViewer.Document.Pages != null)
            {
                var printDialog = new PrintDialog
                {
                    UserPageRangeEnabled = true,
                    MinPage = 1,
                    MaxPage = (uint)pdfViewer.Document.Pages.Count
                };

                if (pdfViewer.Settings.PrintSettings.UseDefaultPrinter || printDialog.ShowDialog() == true)
                {
                    pdfViewer.Print(printDialog, pdfViewer.Settings.PrintSettings);
                    _viewModel.OnPrintReportCompleted();
                }
            }
        }
    }  
}

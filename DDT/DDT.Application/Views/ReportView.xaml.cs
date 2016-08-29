

using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Data;
using Telerik.Windows.Documents.Fixed;

namespace DDT.Application.Views
{
    public partial class ReportView
    {
        public ReportView()
        {
            InitializeComponent();

            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                BindingOperations.SetBinding(this, ReportPDFPathProperty, new Binding("ReportPDFPath"));
                BindingOperations.SetBinding(this, ReportCSVPathProperty, new Binding("ReportCSVPath"));
                BindingOperations.SetBinding(this, ReportActionProperty, new Binding("StopGenerateReportProgress"));
            }
        }

        #region ReportPDFPathProperty (DependencyProperty)

        public string ReportPDFPath
        {
            get { return (string)GetValue(ReportPDFPathProperty); }
            set { SetValue(ReportPDFPathProperty, value); }
        }

        public static readonly DependencyProperty ReportPDFPathProperty =
            DependencyProperty.Register("ReportPDFPath", typeof(string), typeof(ReportView),
            new PropertyMetadata(null, OnReportPDFPathChanged));

        private static void OnReportPDFPathChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var control = obj as ReportView;
            var value = e.NewValue as string;

            control.pdf.DocumentSource = null;
            if (!string.IsNullOrEmpty(value))
                using (var stream = File.OpenRead(value))
                    control.pdf.DocumentSource = new PdfDocumentSource(stream);
        }

        #endregion

        #region ReportCSVPathProperty (DependencyProperty)

        public string ReportCSVPath
        {
            get { return (string)GetValue(ReportCSVPathProperty); }
            set { SetValue(ReportCSVPathProperty, value); }
        }

        public static readonly DependencyProperty ReportCSVPathProperty =
            DependencyProperty.Register("ReportCSVPath", typeof(string), typeof(ReportView),
            new PropertyMetadata(null));

        #endregion

        #region ReportActionProperty (DependencyProperty)

        public Action ReportAction
        {
            get { return (Action)GetValue(ReportActionProperty); }
            set { SetValue(ReportActionProperty, value); }
        }

        public static readonly DependencyProperty ReportActionProperty =
            DependencyProperty.Register("ReportAction", typeof(Action), typeof(ReportView),
            new PropertyMetadata(null));

        #endregion

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            pdf.CommandDescriptors.PrintCommandDescriptor.Command.Execute(null);
        }

        private void pdf_DocumentChanged(object sender, DocumentChangedEventArgs e)
        {
            ReportAction?.Invoke();
            pdf.ScaleFactor = 1.16d;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var saveDialog = new SaveFileDialog();
            saveDialog.Filter = "PDF File|*.pdf|CSV File|*.csv";

            bool? dialogResult = saveDialog.ShowDialog();
            if (dialogResult == true && File.Exists(saveDialog.FilterIndex == 1 ? ReportPDFPath : ReportCSVPath))
            {
                using (var output = File.Create(saveDialog.FileName))
                using (var input = File.OpenRead(saveDialog.FilterIndex == 1 ? ReportPDFPath : ReportCSVPath))
                {
                    input.Seek(0, SeekOrigin.Begin);
                    input.CopyTo(output);
                    output.Flush();
                }
            }
        }
    }
}
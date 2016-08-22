

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
                BindingOperations.SetBinding(this, ReportPathProperty, new Binding("ReportPath"));
                BindingOperations.SetBinding(this, ReportActionProperty, new Binding("StopGenerateReportProgress"));
            }
        }

        #region ReportPathProperty (DependencyProperty)

        public string ReportPath
        {
            get { return (string)GetValue(ReportPathProperty); }
            set { SetValue(ReportPathProperty, value); }
        }

        public static readonly DependencyProperty ReportPathProperty =
            DependencyProperty.Register("ReportPath", typeof(string), typeof(ReportView),
            new PropertyMetadata(null, OnReportPathChanged));

        private static void OnReportPathChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var control = obj as ReportView;
            var value = e.NewValue as string;

            control.pdf.DocumentSource = null;
            if (!string.IsNullOrEmpty(value))
                using (var stream = File.OpenRead(value))
                    control.pdf.DocumentSource = new PdfDocumentSource(stream);
        }

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
            pdf.ScaleFactor = 1.1d;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var saveDialog = new SaveFileDialog();
            saveDialog.Filter = "PDF File|*.pdf";

            bool? dialogResult = saveDialog.ShowDialog();
            if (dialogResult == true && File.Exists(ReportPath))
            {
                using (var output = File.Create(saveDialog.FileName))
                using (var input = File.OpenRead(ReportPath))
                {
                    input.Seek(0, SeekOrigin.Begin);
                    input.CopyTo(output);
                    output.Flush();
                }
            }
        }
    }
}
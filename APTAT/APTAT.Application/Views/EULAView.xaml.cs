
using System;
using System.Windows;
using Telerik.Windows.Documents.Fixed;

namespace APTAT.Application
{
    public partial class EULAView
    {
        public EULAView()
        {
            InitializeComponent();

            DataContext = App.Current.MainWindow.DataContext;
        }

        private void EULAView_Loaded(object sender, RoutedEventArgs e)
        {
            var sri = App.GetResourceStream(new Uri("EULA.pdf", UriKind.Relative));
            if (sri != null)
                Dispatcher.BeginInvoke((Action)delegate
                {
                    using (var stream = sri.Stream)
                        pdf.DocumentSource = new PdfDocumentSource(stream);
                });
        }

        private void pdf_DocumentChanged(object sender, DocumentChangedEventArgs e)
        {
            pdf.ScaleFactor = 1.15d;
        }
    }
}
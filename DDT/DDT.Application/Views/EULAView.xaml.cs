
using System.IO;
using System.Windows;
using Telerik.Windows.Documents.Fixed;

namespace DDT.Application.Views
{
    public partial class EULAView
    {
        public EULAView()
        {
            InitializeComponent();

            Loaded += EULAView_Loaded;
        }

        private void EULAView_Loaded(object sender, RoutedEventArgs e)
        {
            string fileName = @"EULA.pdf";

            if (File.Exists(fileName))
            {
                using (var stream = File.OpenRead(fileName))
                    pdf.DocumentSource = new PdfDocumentSource(stream);
            }
        }

        private void pdf_DocumentChanged(object sender, DocumentChangedEventArgs e)
        {
            pdf.ScaleFactor = 1.16d;
        }
    }
}
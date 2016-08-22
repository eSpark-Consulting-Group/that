
using System.IO;
using System.Windows;
using System.Windows.Documents;

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
            string fileName = @"EULA.rtf";

            if (File.Exists(fileName))
            {
                var textRange = new TextRange(EulaRichTextBox.Document.ContentStart, EulaRichTextBox.Document.ContentEnd);

                using (var stream = File.OpenRead(fileName))
                    textRange.Load(stream, DataFormats.Rtf);
            }
        }
    }
}
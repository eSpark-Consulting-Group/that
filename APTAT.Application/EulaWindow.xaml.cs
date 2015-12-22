using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace APTAT.Application
{
    /// <summary>
    /// Interaction logic for EulaWindow.xaml
    /// </summary>
    public partial class EulaWindow : Window
    {
        public EulaWindow()
        {
            InitializeComponent();
            Loaded += EulaWindow_Loaded;
            Closed += EulaWindow_Closed;
        }

        void EulaWindow_Closed(object sender, EventArgs e)
        {
            if(this.AgreeCheckbox.IsChecked == false) System.Windows.Application.Current.Shutdown();
        }

        void EulaWindow_Loaded(object sender, RoutedEventArgs e)
        {
            string fileName = @"Unknown File Hunter EULA (11-18-2015).rtf";

            if (System.IO.File.Exists(fileName))
            {
                var textRange = new TextRange(EulaRichTextBox.Document.ContentStart, EulaRichTextBox.Document.ContentEnd);
                
                using (System.IO.FileStream fileStream = new System.IO.FileStream(fileName, System.IO.FileMode.OpenOrCreate))
                {
                    textRange.Load(fileStream, System.Windows.DataFormats.Rtf);
                }
            }
        }

        private void OnOkButtonClick(object sender, RoutedEventArgs e)
        {

            this.DialogResult = this.AgreeCheckbox.IsChecked;
            this.Close();

        }
    }
}

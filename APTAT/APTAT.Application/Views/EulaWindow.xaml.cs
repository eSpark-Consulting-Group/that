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
using System.Windows.Shapes;
using Telerik.Windows.Documents.Fixed;
using Telerik.Windows.Documents.Fixed.FormatProviders;
using Telerik.Windows.Documents.Fixed.FormatProviders.Pdf;
using Telerik.Windows.Documents.Fixed.Model;

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
            EulaRichTextBox.Background = new SolidColorBrush(Colors.White);
            EulaRichTextBox.BorderThickness = new Thickness(0);
            BorderThickness = new Thickness(1);
            BorderBrush = new SolidColorBrush(Color.FromRgb(214,212,212));

            var sri = App.GetResourceStream(new Uri("EULA.pdf", UriKind.Relative));
            if (sri != null)
                Dispatcher.BeginInvoke((Action)delegate
                {
                    using (var stream = sri.Stream)
                        EulaRichTextBox.DocumentSource = new PdfDocumentSource(stream);
                });
        }

        private void OnOkButtonClick(object sender, RoutedEventArgs e)
        {
            this.DialogResult = this.AgreeCheckbox.IsChecked;
            this.Close();

        }

        private void OnCancelButtonClick(object sender, RoutedEventArgs e)
        {
            App.Current.Shutdown();

        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            DragMove();
        }

    }
}

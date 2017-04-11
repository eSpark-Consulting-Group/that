using FAT.Application.Helpers;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Telerik.Windows.Documents.Fixed;

namespace FAT.Application
{
    public partial class EULAWindow
    {
        public EULAWindow()
        {
            InitializeComponent();

            DataContext = App.Current.MainWindow.DataContext;
        }

        private void EULAView_Loaded(object sender, RoutedEventArgs e)
        {
            ThreadHelper.Instance.RunOnThread(delegate
            {
                var sri = App.GetResourceStream(new Uri("EULA.pdf", UriKind.Relative));
                if (sri != null)
                {
                    Dispatcher.BeginInvoke((Action)delegate
                    {
                        using (var stream = sri.Stream)
                            pdf.DocumentSource = new PdfDocumentSource(stream);
                    }, DispatcherPriority.Background);
                }
            });
            Activate();
        }

        private void pdf_DocumentChanged(object sender, DocumentChangedEventArgs e)
        {
            pdf.ScaleFactor = 1.19d;
        }

        private void Window_MouseLeftButtonDown(Object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void YesClick(Object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void NoClick(Object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            App.Current.Shutdown();
        }

        private async void Hyperlink_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var location = Owner.PointToScreen(new Point(-3, -3));

            eula.Opacity = 0;
            pdf.MaxHeight = 700;

            await Dispatcher.Yield(DispatcherPriority.Background);

            await Dispatcher.BeginInvoke((Action)delegate
            {
                Left = location.X;
                Top = location.Y;
                Width = Owner.ActualWidth + 6;
                MaxHeight = Owner.ActualHeight + 6;
            });
        }
    }
}
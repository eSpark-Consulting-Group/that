using UFH.Application.Helpers;
using System;
using System.Windows;
using System.Windows.Input;
using Telerik.Windows.Documents.Fixed;

namespace UFH.Application
{
    public partial class EULAWindow
    {
        public EULAWindow()
        {
            InitializeComponent();

            DataContext = App.Current.MainWindow.DataContext;
        }

        private EULAFullWindow _full;

        private void EULAView_Loaded(object sender, RoutedEventArgs e)
        {
            _full = new EULAFullWindow();
            _full.Owner = Owner;
            ThreadHelper.Instance.RunOnThread(delegate
            {
                var res = App.GetResourceStream(new Uri("EULA.pdf", UriKind.Relative));
                if (res != null)
                    _full.Dispatcher.BeginInvoke((Action)delegate
                    {
                        using (var stream = res.Stream)
                            _full.pdf.DocumentSource = new PdfDocumentSource(stream);
                    });
            });
            Activate();
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

        private void Hyperlink_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Dispatcher.BeginInvoke((Action)delegate
            {
                _full.ShowDialog();
            });
            Close();
        }
    }
}
using APTAT.Application.Extensions;
using APTAT.Application.Models;
using System;
using System.Windows;
using System.Windows.Input;


namespace APTAT.Application
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
        }


        #region ShowCredentialsProperty (DependencyProperty)
        public bool ShowCredentials
        {
            get { return (bool)GetValue(ShowCredentialsProperty); }
            set { SetValue(ShowCredentialsProperty, value); }
        }

        public static readonly DependencyProperty ShowCredentialsProperty =
            DependencyProperty.Register("ShowCredentials", typeof(bool), typeof(SettingsWindow),
            new PropertyMetadata(false));

        #endregion

        #region HeaderProperty (DependencyProperty)
        public string Header
        {
            get { return (string)GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register("Header", typeof(string), typeof(SettingsWindow),
            new PropertyMetadata("Options"));

        #endregion


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MainViewViewModel model = DataContext as MainViewViewModel;
            string message;
            if (!model.ValidatePorts(out message))
            {
                model.Error = message;
                return;
            }

            this.DialogResult = true;

        }

        private void Button_CancelClick(object sender, RoutedEventArgs e)
        {
            tcp.Text = _tcp;
            udp.Text = _udp;
            this.DialogResult = false;
        }

        private void Window_MouseLeftButtonDown(Object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void OnCloseClick(object sender, RoutedEventArgs e)
        {
            tcp.Text = _tcp;
            udp.Text = _udp;
            DialogResult = false;
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            PortValidationExtensions.ValidateInput(sender, e);
        }

        private string _tcp, _udp;
        private void this_Loaded(object sender, RoutedEventArgs e)
        {

            Dispatcher.BeginInvoke((Action)delegate
            {
                _tcp = tcp.Text;
                _udp = udp.Text;
            });
        }
    }
}

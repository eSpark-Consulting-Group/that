using APTAT.Application.Extensions;
using APTAT.Application.Models;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Telerik.Windows.Controls;

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
            if (!ShowCredentials && !model.ValidatePorts(out message))
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

        private string _tcp, _udp;

        private void PortKeyDown(object sender, KeyEventArgs e)
        {
            bool flag = (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9) || (e.Key >= Key.D0 && e.Key <= Key.D9);

            if (e.Key == Key.Tab || e.Key == Key.Enter || e.Key == Key.Delete || e.Key == Key.Back)
                flag = true;

            if ((e.Key == Key.NumPad0 || e.Key == Key.D0) && string.IsNullOrEmpty((sender as TextBox).Text))
                flag = false;

            if (!flag)
                e.Handled = true;
        }

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


using UFH.Application.Models;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace UFH.Application
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

        #region ShowProxyCredentialsProperty (DependencyProperty)
        public bool ShowProxyCredentials
        {
            get { return (bool)GetValue(ShowProxyCredentialsProperty); }
            set { SetValue(ShowProxyCredentialsProperty, value); }
        }

        public static readonly DependencyProperty ShowProxyCredentialsProperty =
            DependencyProperty.Register("ShowProxyCredentials", typeof(bool), typeof(SettingsWindow),
            new PropertyMetadata(false));

        #endregion

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

        #region ShowScanModesProperty (DependencyProperty)
        public bool ShowScanModes
        {
            get { return (bool)GetValue(ShowScanModesProperty); }
            set { SetValue(ShowScanModesProperty, value); }
        }

        public static readonly DependencyProperty ShowScanModesProperty =
            DependencyProperty.Register("ShowScanModes", typeof(bool), typeof(SettingsWindow),
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

        public bool IsQuickScan { get; set; }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var model = DataContext as MainViewModel;
            string message;
            if (!ShowProxyCredentials && !ShowCredentials && !model.ValidatePorts(out message))
            {
                model.Error = message;
                return;
            }

            DialogResult = true;
        }

        private void ButtonQuick_Click(object sender, RoutedEventArgs e)
        {
            var model = DataContext as WizardViewModel;
            model.ChangeStepCommand.Execute(null);
            if (!model.HasErrors)
            {
                IsQuickScan = true;
                DialogResult = true;
            }
        }

        private void ButtonFull_Click(object sender, RoutedEventArgs e)
        {
            var model = DataContext as WizardViewModel;
            model.ChangeStepCommand.Execute(null);
            if (!model.HasErrors)
            {
                DialogResult = true;
            }
        }

        private bool _warningDisplayed;

        private void Button_CancelClick(object sender, RoutedEventArgs e)
        {
            var model = Owner.DataContext as MainViewModel;
            if (ShowProxyCredentials)
            {
                if (!_warningDisplayed)
                {
                    _warningDisplayed = true;
                    bord.Visibility = Visibility.Visible;
                    if (model.IsScanInProgress.GetValueOrDefault())
                        txt.Text = "You need to provide proxy credentials to complete file analysis in cloud. Please enter valid proxy login and password, otherwise the current operation will be aborted.";
                    return;
                }
            }
            else
            {
                tcp.Text = _tcp;
                udp.Text = _udp;
            }
            DialogResult = false;
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
            bool shift = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
            bool ctrl = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);

            bool flag = (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9) || (e.Key >= Key.D0 && e.Key <= Key.D9 && !shift && !ctrl);

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

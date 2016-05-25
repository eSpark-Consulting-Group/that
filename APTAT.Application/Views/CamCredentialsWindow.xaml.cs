using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using APTAT.Application.Models;

namespace APTAT.Application
{
    /// <summary>
    /// Interaction logic for CamCredentialsWindow.xaml
    /// </summary>
    public partial class CamCredentialsWindow : Window
    {
        private readonly ValkyrieRegistrationModel _model;
        private readonly Brush _defaultBorderBrush;
        public CamCredentialsWindow(ValkyrieRegistrationModel model)
        {
            InitializeComponent();
            _model = model;
            this.RegistrationFormLayout.DataContext = model;
            
            _defaultBorderBrush = PasswordText.BorderBrush;
        }

        private void OnCloseClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        public void MarkPasswordValidation(bool isValid)
        {
            if (isValid)
            {
                this.PasswordText.BorderBrush = _defaultBorderBrush;
            }
            else
            {
                this.PasswordText.BorderBrush  = new SolidColorBrush(Colors.Red);
            }
        }

        public event EventHandler OkClick;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OnOkClick();
        }

        public void SetErrorMessage(string errorMessage)
        {
            if (_model == null) return;
            _model.ErrorMessage = errorMessage;
        }

        public string LicenseId
        {
            get { return this.LicenseIdText.Text.Trim(); }
        }

        public string UserName
        {
            get { return this.LoginText.Text.Trim(); }
        }

        private void GetValkyrieButton_OnClick(object sender, RoutedEventArgs e)
        {
            var resource = TryFindResource("GETValkyrieLink");
            Process.Start(new ProcessStartInfo(resource.ToString()));
            e.Handled = true;
        }

        private void OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            _model.Password = this.PasswordText.Password.Trim();
//            PasswordText.InvalidateVisual();
        }

        protected virtual void OnOkClick()
        {
            _model.ErrorMessage = String.Empty;
            var handler = OkClick;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        private void Hyperlink_OnRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private void CamCredentialsWindow_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
           DragMove();
        }
    }
}

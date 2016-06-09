using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;
using APTAT.Application.Helpers;
using APTAT.Application.Models;

namespace APTAT.Application
{
    /// <summary>
    /// Interaction logic for CamCredentialsWindow.xaml
    /// </summary>
    public partial class CamCredentialsWindow : Window
    {
        private readonly ValkyrieRegistrationModel _model;

        public CamCredentialsWindow()
        {
            InitializeComponent();
            DataContext = _model = CompositionHelper.Instance.GetService<ValkyrieRegistrationModel>();
            _model.Password = null; 
            _model.ErrorMessage = null;
        }

        private void OnCloseClick(object sender, RoutedEventArgs e)
        {
            Close();
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

        private void GetValkyrieButton_OnClick(object sender, RoutedEventArgs e)
        {
            var resource = TryFindResource("GETValkyrieLink");
            Process.Start(new ProcessStartInfo(resource.ToString()));
            e.Handled = true;
        }

        private void OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            _model.Password = PasswordText.Password;
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

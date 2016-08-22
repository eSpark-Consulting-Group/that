using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;
using APTAT.Application.Helpers;
using APTAT.Application.Models;
using APTAT.Common;

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
            _model.PasswordBuffer = null; 
            _model.ErrorMessage = null;
        }

        private void OnCloseClick(object sender, RoutedEventArgs e)
        {
            OnCancelClick();
        }

        public event EventHandler OkClick;
        public event EventHandler CancelClick;

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

        protected virtual void OnOkClick()
        {
            _model.ErrorMessage = String.Empty;
            var handler = OkClick;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        protected virtual void OnCancelClick()
        {
            _model.ErrorMessage = String.Empty;
            var handler = CancelClick;
            if (handler != null) handler(this, EventArgs.Empty);

            Close();
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

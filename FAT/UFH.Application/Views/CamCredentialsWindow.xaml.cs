using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;

using APTAT.Common;
using APTAT.COT.Login;
using UFH.Application.Models;

namespace UFH.Application
{
    /// <summary>
    /// Interaction logic for CamCredentialsWindow.xaml
    /// </summary>
    public partial class CamCredentialsWindow : Window
    {
        private readonly ValkyrieRegistrationModel _model;
        public event EventHandler OkClick;
        public event EventHandler CancelClick;
        public event EventHandler OnClosedEvent;
        public event EventHandler OnClosingEvent;
        public event EventHandler OnGuestButtonClick;

        public CamCredentialsWindow()
        {
            InitializeComponent();
            DataContext = _model = CompositionHelper.Instance.Resolve<ValkyrieRegistrationModel>();
            _model.Password = null;
            _model.PasswordBuffer = null;
            _model.ErrorMessage = null;            
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            OnClosedEvent?.Invoke(this, e);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            OnClosingEvent?.Invoke(this, e);
        }

        private void OnCloseClick(object sender, RoutedEventArgs e)
        {
            OnCancelClick();
        }

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
            OkClick?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnCancelClick()
        {
            _model.ErrorMessage = String.Empty;
            CancelClick?.Invoke(this, EventArgs.Empty);

            Close();
        }

        private void Hyperlink_OnRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private void Guest_OnRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            _model.ErrorMessage = String.Empty;
            OnGuestButtonClick?.Invoke(this, e);
        }

        private void CamCredentialsWindow_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {

        }
    }
}

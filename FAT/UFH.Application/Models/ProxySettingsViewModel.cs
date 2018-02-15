using System;
using System.Net;

namespace UFH.Application.Models
{
    public interface IProxySettingsViewModel
    {
        NetworkCredential GetProxyCredentials();
    }
    public class ProxySettingsViewModel : IProxySettingsViewModel
    {
        public string ProxyLogin { get; set; }
        public string ProxyPassword { get; set; }
        public NetworkCredential GetProxyCredentials()
        {
            bool retry = false;
            App.Current.Dispatcher.Invoke(() =>
            {
                SettingsWindow window = new SettingsWindow();
                window.Owner = System.Windows.Application.Current.MainWindow;
                window.DataContext = this;
                window.ShowProxyCredentials = true;
                window.Header = "Proxy Credentials";

                retry = window.ShowDialog().GetValueOrDefault();
            });
            if (!retry)
                return null;
            return new NetworkCredential(ProxyLogin, ProxyPassword, null);
        }
    }
}

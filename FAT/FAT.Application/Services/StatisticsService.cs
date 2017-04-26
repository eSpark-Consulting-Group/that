using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using APTAT.Core.CMC;
using APTAT.Core.CMC.Request;
using FAT.Application.Models;
using FAT.Application.Properties;
using System.Reflection;
using FAT.Application.Messages;
using static FAT.Application.Messages.AppMessages;

namespace FAT.Application.Services
{
    public interface IStatisticsService
    {
        void ApplicationLaunchedAsync(IApplicationLaunchedRequest request);
        void AutoDiscoveryFinishedAsync(IAutoDiscoveryFinishedRequest request);
        void ScanStartedAsync(IScanStartedRequest request);
        void ScanFinishedAsync(IScanFinishedRequest request);
        void ScanUpdatedAsync(IScanUpdatedRequest request);
        void ReportGeneratedAsync(IReportGeneratedRequest request);
        void ReportPrintedOrSavedAsync(IReportPrintedOrSavedRequest request);
        void ApplicationClosedAsync(IApplicationClosedRequest request);
    }

    public class SendDataResponse {
        public bool IsSuccess { get; set; }
        public Exception SendDataException { get; set; }
    }

    public class StatisticsService : IStatisticsService
    {
        private IProxySettingsViewModel _proxyCredentialsProvider;
        private ICmcClient _client;
        private object lockObj = new object();
        private bool _isEnabledSendData=true;
        private NetworkCredential _proxyCredentials;

        public StatisticsService(IProxySettingsViewModel settingsViewModel, ICmcClient client)
        {
            _proxyCredentialsProvider = settingsViewModel;
            _client = client;
            if (Settings.Default.cmcStatistics == "Disabled")
            _isEnabledSendData = false;

            MessageManager.Instance.RegisterListener(this);
        }

        #region Message Receivers

        [MessageReceiver]
        public void AcceptMessage(AppMessage<IApplicationLaunchedRequest> message)
        {
            var request = new ApplicationLaunchedRequest();
            message.Action?.Invoke(request);
            ApplicationLaunchedAsync(request);
        }

        [MessageReceiver]
        public void AcceptMessage(AppMessage<IAutoDiscoveryFinishedRequest> message)
        {
            var request = new AutoDiscoveryFinishedRequest();
            message.Action?.Invoke(request);
            AutoDiscoveryFinishedAsync(request);
        }

        [MessageReceiver]
        public void AcceptMessage(AppMessage<IScanStartedRequest> message)
        {
            var request = new ScanStartedRequest();
            message.Action?.Invoke(request);
            ScanStartedAsync(request);
        }

        [MessageReceiver]
        public void AcceptMessage(AppMessage<IScanFinishedRequest> message)
        {
            var request = new ScanFinishedRequest();
            message.Action?.Invoke(request);
            ScanFinishedAsync(request);
        }

        [MessageReceiver]
        public void AcceptMessage(AppMessage<IScanUpdatedRequest> message)
        {
            var request = new ScanUpdatedRequest();
            message.Action?.Invoke(request);
            ScanUpdatedAsync(request);
        }

        [MessageReceiver]
        public void AcceptMessage(AppMessage<IReportGeneratedRequest> message)
        {
            var request = new ReportGeneratedRequest();
            message.Action?.Invoke(request);
            ReportGeneratedAsync(request);
        }

        [MessageReceiver]
        public void AcceptMessage(AppMessage<IReportPrintedOrSavedRequest> message)
        {
            var request = new ReportPrintedOrSavedRequest();
            message.Action?.Invoke(request);
            ReportPrintedOrSavedAsync(request);
        }

        [MessageReceiver]
        public void AcceptMessage(AppMessage<IApplicationClosedRequest> message)
        {
            var request = new ApplicationClosedRequest();
            message.Action?.Invoke(request);
            ApplicationClosedAsync(request);
        }

        #endregion

        private SendDataResponse TryCallAsync(Action action)
        {
            //Task.Factory.StartNew(() =>
            //{
                lock (lockObj)
                {
                    int attemptsCounter = 0;
                    if (_isEnabledSendData)
                    {
                        SendDataResponse firstException = null;
                        while (true)
                        {
                            try
                            {
                                action.Invoke();
                                return new SendDataResponse() { IsSuccess = true, SendDataException = null };
                            }
                            catch (Exception ex)
                            {
                                var webException = ex as WebException;
                                var httpWebResponse = webException?.Response as HttpWebResponse;
                                HttpStatusCode? code = httpWebResponse?.StatusCode;
                                if (code == HttpStatusCode.ProxyAuthenticationRequired)
                                {
                                    _proxyCredentials = _proxyCredentialsProvider.GetProxyCredentials();
                                    if (_proxyCredentials != null)
                                    {
                                        attemptsCounter = 0;
                                        continue;
                                    }
                                }
                                if (attemptsCounter == 3)
                                {
                                    return firstException;
                                }
                                if (firstException == null)
                                {
                                    firstException = new SendDataResponse() { IsSuccess = false, SendDataException = ex };
                                }
                                attemptsCounter++;
                                Thread.Sleep(1000);
                            }
                        }
                    }
                    return new SendDataResponse() { IsSuccess = false, SendDataException = new Exception() };
                }
            //});
        }

        public void ApplicationLaunchedAsync(IApplicationLaunchedRequest request)
        {
            Action action = () =>
            {
                if (File.Exists("originalfile"))
                {
                    string input = File.ReadAllText("originalfile");
                    string pattern = @"ForensicAnalysisTool_\w{32}$";
                    Regex rgx = new Regex(pattern, RegexOptions.IgnoreCase);
                    var matches = rgx.Matches(input);
                    if (matches.Count == 1)
                    {
                        var result = matches[0].Value.Split(new[] { '_' })[1].Split(new[] { '.' })[0];
                        request.AcId = Encoding.Default.GetString(Convert.FromBase64String(result));
                    }
                }
                request.ProductIdentifier = "enp.c1.fat";
                request.ProductVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
                _client.ApplicationLaunched(request, _proxyCredentials);
            };
            TryCallAsync(action);
        }

        public void AutoDiscoveryFinishedAsync(IAutoDiscoveryFinishedRequest request)
        {
            Action action = () => { _client.AutoDiscoveryFinished(request, _proxyCredentials); };
            TryCallAsync(action);
        }

        public void ScanStartedAsync(IScanStartedRequest request)
        {
            Action action = () => { _client.ScanStarted(request, _proxyCredentials); };
            TryCallAsync(action);
        }

        public void ScanFinishedAsync(IScanFinishedRequest request)
        {
            Action action = () => { _client.ScanFinished(request, _proxyCredentials); };
            TryCallAsync(action);
        }

        public void ScanUpdatedAsync(IScanUpdatedRequest request)
        {
            Action action = () => { _client.ScanUpdated(request, _proxyCredentials); };
            TryCallAsync(action);
        }

        public void ReportGeneratedAsync(IReportGeneratedRequest request)
        {
            Action action = () => { _client.ReportGenerated(request, _proxyCredentials); };
            TryCallAsync(action);
        }

        public void ReportPrintedOrSavedAsync(IReportPrintedOrSavedRequest request)
        {
            Action action = () => { _client.ReportPrintedOrSaved(request, _proxyCredentials); };
            TryCallAsync(action);
        }

        public void ApplicationClosedAsync(IApplicationClosedRequest request)
        {
            Action action = () => { _client.ApplicationClosed(request, _proxyCredentials); };
            TryCallAsync(action);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using APTAT.Core.CMC;
using APTAT.Core.CMC.Request;
using APTAT.Core.CMC.UFH;
using APTAT.Core.Properties;
using FAT.Application.Models;
using FAT.Application.Properties;

namespace FAT.Application.Services
{
    public interface IStatisticsService
    {
        void LaunchedAsync(ILaunchedRequest request);
        void AutoDiscoveryFinishedAsync(IAutoDiscoveryFinishedRequest request);
        void ScanStartedAsync(IScanStartedRequest request);
        void ScanFinishedAsync(IScanFinishedRequest request);
        void ScanUpdatedAsync(IScanUpdatedRequest request);
        void ReportGeneratedAsync(IReportGeneratedRequest request);
        void ReportPrintedOrSavedAsync(IReportPrintedOrSavedRequest request);
        void AppicationClosedAsync(IApplicationClosedRequest request);
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
        }

        private void TryCallAsync(Action action)
        {
            Task.Factory.StartNew(() =>
            {
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
            });
        }

        public void LaunchedAsync(ILaunchedRequest request)
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
                _client.Launched(request, _proxyCredentials);
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

        public void AppicationClosedAsync(IApplicationClosedRequest request)
        {
            Action action = () => { _client.AppicationClosed(request, _proxyCredentials); };
            TryCallAsync(action);
        }
    }
}

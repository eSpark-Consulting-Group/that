using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using APTAT.Core.CMC;
using APTAT.Core.CMC.Request;
using FAT.Application.Models;
using FAT.Application.Properties;
using System.Reflection;
using APTAT.Common;
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
        Task ApplicationClosed(IApplicationClosedRequest request);
    }

    public class SendDataResponse
    {
        public bool IsSuccess { get; set; }
        public Exception SendDataException { get; set; }
    }

    public class StatisticsService : IStatisticsService
    {
        private readonly IProxySettingsViewModel _proxyCredentialsProvider;
        private readonly ICmcClient _client;
        private readonly object _lockObj = new object();

        private NetworkCredential _proxyCredentials;
        private ConcurrentQueue<Action> _queue = new ConcurrentQueue<Action>();
        private bool _isNotStarted = true;
        private readonly bool _isEnabledSendData = true;
        private bool _askCredentionals = true;

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

        public async Task ApplicationClosed(IApplicationClosedRequest request)
        {
            await Task.Factory.StartNew(t =>
            {
               _client.ApplicationClosed(request, _proxyCredentials);
            }, TaskCreationOptions.None);
        }

        private void ProduceRequest(Action action)
        {
            if (!_isEnabledSendData) return;
            _queue.Enqueue(action);

            bool shouldStart;
            lock (_lockObj)
            {
                shouldStart = _isNotStarted;
            }
            if (shouldStart)
            {
                Task.Factory.StartNew(ConsumeRequest);
            }
        }

        void ConsumeRequest()
        {
            lock (_lockObj)
            {
                if (!_isNotStarted)
                    return;
            }
            while (true)
            {
                Action action;
                if (_queue.TryDequeue(out action))
                {
                    while (true)
                    {
                        try
                        {
                            action.Invoke();
                        }
                        catch (Exception ex)
                        {
                            var webException = ex as WebException;
                            var httpWebResponse = webException?.Response as HttpWebResponse;
                            HttpStatusCode? code = httpWebResponse?.StatusCode;
                            if (code == HttpStatusCode.ProxyAuthenticationRequired && _askCredentionals)
                            {
                                _proxyCredentials = _proxyCredentialsProvider.GetProxyCredentials();
                                if (_proxyCredentials != null)
                                {
                                    continue;
                                }
                                _askCredentionals = false;
                            }

                            LogHelper.Trace(TraceEventType.Error,
                                $"Statistics service warning: {ex.GetType()} {ex.Message}{Environment.NewLine}{ex.StackTrace}");
                            Thread.Sleep(1000);
                        }
                    }
                }
                else
                {
                    break;
                }
            }
            lock (_lockObj)
            {
                _isNotStarted = true;
                if (!_queue.IsEmpty) ConsumeRequest();
            }
        }

        public void ApplicationLaunchedAsync(IApplicationLaunchedRequest request)
        {
            Action action =() =>
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
                        var id = Encoding.Default.GetString(Convert.FromBase64String(result));
                        rgx = new Regex(@"[0-9a-zA-Z]{24}");
                        if (rgx.IsMatch(id))
                        {
                            request.AcId = id;
                        }
                    }
                }
                request.ProductIdentifier = "enp.c1.fat";
                request.ProductVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
                _client.ApplicationLaunched(request, _proxyCredentials);
            };
                ProduceRequest(action);
        }

        public void AutoDiscoveryFinishedAsync(IAutoDiscoveryFinishedRequest request)
        {
            Action action = () => { _client.AutoDiscoveryFinished(request, _proxyCredentials); };
            ProduceRequest(action);
        }

        public void ScanStartedAsync(IScanStartedRequest request)
        {
            Action action = () => { _client.ScanStarted(request, _proxyCredentials); };
            ProduceRequest(action);
        }

        public void ScanFinishedAsync(IScanFinishedRequest request)
        {
            Action action = () => { _client.ScanFinished(request, _proxyCredentials); };
            ProduceRequest(action);
        }

        public void ScanUpdatedAsync(IScanUpdatedRequest request)
        {
            Action action = () => { _client.ScanUpdated(request, _proxyCredentials); };
                ProduceRequest(action);
        }

        public void ReportGeneratedAsync(IReportGeneratedRequest request)
        {
            Action action = () => { _client.ReportGenerated(request, _proxyCredentials); };
            ProduceRequest(action);
        }

        public void ReportPrintedOrSavedAsync(IReportPrintedOrSavedRequest request)
        {
            Action action = () => { _client.ReportPrintedOrSaved(request, _proxyCredentials); };
            ProduceRequest(action);
        }

        public void ApplicationClosedAsync(IApplicationClosedRequest request)
        {
            Action action = () => { _client.ApplicationClosed(request, _proxyCredentials); };
            ProduceRequest(action);
        }
    }
}

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
using Valkyrie.WebApiProvider;
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
        Task OnAppClosing();
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

        
        private ConcurrentQueue<Action> _queue = new ConcurrentQueue<Action>();
        private bool _isNotStarted = true;
        private bool _isEnabledSendData = true;
        private bool _askCredentials = true;
        private IScheduler _sendUpdateTimer;

        private int _malwareDiffCount = 0;
        private int _unknownDiffCount = 0;

        private object _objCountersLock=new object(); 

        public StatisticsService(IProxySettingsViewModel settingsViewModel, ICmcClient client, IScheduler timer)
        {
            _proxyCredentialsProvider = settingsViewModel;
            _client = client;

            _isEnabledSendData = Settings.Default.cmcStatistics;

            MessageManager.Instance.RegisterListener(this);

            _sendUpdateTimer = timer;
            if (_sendUpdateTimer != null)
            {
                _sendUpdateTimer.SetInterval(TimeSpan.FromMilliseconds(Settings.Default.sendUpdateTimerIntervalMs));
                _sendUpdateTimer.TimeoutCallback += SendUpdateScanByTimerCallback;
                _sendUpdateTimer.Start();
            }

        }

        public bool IsEnabledSendData
        {
            get { return _isEnabledSendData; }
            internal set
            {
                _isEnabledSendData = value;
            }
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

        [MessageReceiver]
        public void AcceptMessage(AppMessage<IEmailActionRequest> message)
        {
            var request = new EmailActionRequest();
            message.Action?.Invoke(request);
            Action action = () => { _client.EmailAction(request); };
            ProduceRequest(action);
        }
        #endregion

        public async Task OnAppClosing()
        {
            await Task.Run(() =>
            {
                var request = new ApplicationClosedRequest();
                try
                {
                    _client.ApplicationClosed(request);
                }
                catch (Exception ex)
                {
                    LogHelper.Trace(TraceEventType.Error,
                            $"Statistics service warning: {ex.GetType()} {ex.Message}{Environment.NewLine}{ex.StackTrace}");
                }

            });
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
                ConsumeRequest();
            }
        }

        void ConsumeRequest()
        {
            lock (_lockObj)
            {
                if (!_isNotStarted)
                    return;
                _isNotStarted = false;
            }
            
            Action action;
            while (_queue.TryDequeue(out action))
            {
                while (true)
                {
                    try
                    {
                        action.Invoke();
                        break;
                    }
                    catch (Exception ex)
                    {
                        var webException = ex as WebException;
                        var httpWebResponse = webException?.Response as HttpWebResponse;
                        HttpStatusCode? code = httpWebResponse?.StatusCode;
                        if (code == HttpStatusCode.ProxyAuthenticationRequired)
                        {
                            if (_askCredentials)
                            {
                                var proxyCredentials = _proxyCredentialsProvider.GetProxyCredentials();
                                if (proxyCredentials != null)
                                {
                                    ProxySettings.Proxy.Credentials = proxyCredentials;
                                    continue;
                                }
                            }
                            _askCredentials = false;
                        }

                        LogHelper.Trace(TraceEventType.Error,
                            $"Statistics service warning: {ex.GetType()} {ex.Message}{Environment.NewLine}{ex.StackTrace}");
                        Thread.Sleep(TimeSpan.FromSeconds(1));
                    }
                }
            }
            bool shouldStart = false;
            lock (_lockObj)
            {
                _isNotStarted = true;
                if (!_queue.IsEmpty)
                    shouldStart = true;
            }
            if (shouldStart)
                ConsumeRequest();
        }

        public void ApplicationLaunchedAsync(IApplicationLaunchedRequest request)
        {
            Action action = () =>
            {
                if (Assembly.GetEntryAssembly() != null)
                {   
                    var folderPath = new DirectoryInfo(Assembly.GetEntryAssembly().Location).Parent.FullName;
                    var filePath = $"{folderPath}\\originalfile";

                    if (File.Exists(filePath))
                    {
                        string input = File.ReadAllText(filePath);
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
                }
                request.ProductIdentifier = "enp.c1.fat";
                request.ProductVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
                _client.ApplicationLaunched(request);
            };
            ProduceRequest(action);
        }
        

        public void AutoDiscoveryFinishedAsync(IAutoDiscoveryFinishedRequest request)
        {
            Action action = () => { _client.AutoDiscoveryFinished(request); };
            ProduceRequest(action);
        }

        public void ScanStartedAsync(IScanStartedRequest request)
        {
            Action action = () => { _client.ScanStarted(request); };
            ProduceRequest(action);
        }

        public void ScanFinishedAsync(IScanFinishedRequest request)
        {
            Action action = () => { _client.ScanFinished(request ); };
            ProduceRequest(action);
        }

        public void ScanUpdatedAsync(IScanUpdatedRequest request)
        {
            lock (_objCountersLock)
            {
                _malwareDiffCount += request.CountOfNewMaliciousFilesAfterTheScanWasFinished;
                _unknownDiffCount += request.CountOfNewUnknownFilesAfterTheScanWasFinished;
            }
        }

        public void ReportGeneratedAsync(IReportGeneratedRequest request)
        {
            Action action = () => { _client.ReportGenerated(request ); };
            ProduceRequest(action);
        }

        public void ReportPrintedOrSavedAsync(IReportPrintedOrSavedRequest request)
        {
            Action action = () => { _client.ReportPrintedOrSaved(request ); };
            ProduceRequest(action);
        }

        public void ApplicationClosedAsync(IApplicationClosedRequest request)
        {
            Action action = () => { _client.ApplicationClosed(request); };
            ProduceRequest(action);
        }

        private void SendUpdateScanByTimerCallback(object source, EventArgs eventArgs)
        {
            int malwareDiffCount = 0;
            int unknownDiffCount = 0;
            lock (_objCountersLock)
            {
                malwareDiffCount = _malwareDiffCount;
                unknownDiffCount = _unknownDiffCount;

                _malwareDiffCount = 0;
                _unknownDiffCount = 0;
            }

            if(malwareDiffCount==0 && unknownDiffCount == 0) return;
            

            IScanUpdatedRequest request = new ScanUpdatedRequest()
            {
                CountOfNewMaliciousFilesAfterTheScanWasFinished = malwareDiffCount,
                CountOfNewUnknownFilesAfterTheScanWasFinished = unknownDiffCount
            };
            
            Action action = () => { _client.ScanUpdated(request ); };
                ProduceRequest(action);
        }

    }
}

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using APTAT.Core.CMC;
using APTAT.Core.CMC.Request;
using System.Reflection;
using System.Windows;
using APTAT.Application;
using APTAT.Application.Messages;
using APTAT.Application.Properties;
using APTAT.Application.Views;
using APTAT.Common;
using Valkyrie.WebApiProvider;

namespace FAT.Application.Services
{

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class MessageReceiverAttribute : Attribute
    {
    }

    public class MessageManager
    {
        class MessageContract : IEquatable<MessageContract>
        {
            readonly Type[] _parametersTypes;

            public MessageContract(Type[] parametersTypes)
            {
                _parametersTypes = parametersTypes;
            }

            public bool Equals(MessageContract other)
            {
                return other.GetHashCode() == GetHashCode();
            }

            public override bool Equals(object obj)
            {
                if (!(obj is MessageContract))
                {
                    return false;
                }
                return Equals(obj as MessageContract);
            }

            public override int GetHashCode()
            {
                string concat = _parametersTypes.Aggregate(string.Empty, (current, t) => string.Concat(current, t.FullName));

                return concat.GetHashCode();
            }
        }

        private object _sync = new object();

        static readonly Dictionary<Type, IList<WeakReference>> _typeAndVmMap;
        static readonly Dictionary<MessageContract, IList<Tuple<Type, MethodInfo>>> _messageContractAndTypeMethodMap;
        static readonly List<Type> _analyzedTypes;

        public static MessageManager Instance { get; private set; }

        MessageManager()
        {
        }

        static MessageManager()
        {
            _typeAndVmMap = new Dictionary<Type, IList<WeakReference>>();
            _messageContractAndTypeMethodMap = new Dictionary<MessageContract, IList<Tuple<Type, MethodInfo>>>();
            _analyzedTypes = new List<Type>();

            Instance = new MessageManager();
        }

        public void RegisterListener(object obj)
        {
            Type viewModelType = obj.GetType();

            lock (_sync)
            {
                if (!_analyzedTypes.Contains(viewModelType))
                {
                    var methodAndContractMap = CollectMessagingContracts(viewModelType);
                    if (methodAndContractMap.Count == 0)
                        return;

                    foreach (var info in methodAndContractMap.Keys)
                    {
                        var contract = methodAndContractMap[info];
                        if (!_messageContractAndTypeMethodMap.ContainsKey(contract))
                            _messageContractAndTypeMethodMap[contract] = new List<Tuple<Type, MethodInfo>>();

                        _messageContractAndTypeMethodMap[contract].Add(Tuple.Create(viewModelType, info));
                    }
                }
                if (!_typeAndVmMap.ContainsKey(viewModelType))
                    _typeAndVmMap[viewModelType] = new List<WeakReference>();

                _typeAndVmMap[viewModelType].Add(new WeakReference(obj));
                _analyzedTypes.Add(viewModelType);
            }
        }

        IDictionary<MethodInfo, MessageContract> CollectMessagingContracts(Type viewModelSourceType)
        {
            var result = new Dictionary<MethodInfo, MessageContract>();

            var applicableMethods = viewModelSourceType.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).
                Where(e => e.GetCustomAttributes(true).OfType<MessageReceiverAttribute>().Any() && e.ReturnType == typeof(void) && e.GetParameters().Length > 0).ToList();

            applicableMethods.ForEach(e => result.Add(e, new MessageContract(e.GetParameters().Select(p => p.ParameterType).ToArray())));

            return result;
        }

        public void PublishMessage(params object[] messageParameters)
        {
            var inputContract = new MessageContract(messageParameters.Select(e => e.GetType()).ToArray());

            if (!_messageContractAndTypeMethodMap.ContainsKey(inputContract))
                return;

            foreach (var tuple in _messageContractAndTypeMethodMap[inputContract])
                foreach (WeakReference viewModelReference in _typeAndVmMap[tuple.Item1])
                {
                    if (viewModelReference.IsAlive && viewModelReference.Target != null)
                    {
                        var viewModel = viewModelReference.Target;

                        Action action = () => tuple.Item2.Invoke(viewModel, messageParameters);

                        if (viewModel is UIElement)
                            ((UIElement)viewModel).Dispatcher.BeginInvoke(action);
                        else ThreadPool.QueueUserWorkItem(e => action());
                    }
                }
        }
    }
}

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

    private object _objCountersLock = new object();

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
    public void AcceptMessage(AppMessages.AppMessage<IApplicationLaunchedRequest> message)
    {
        var request = new ApplicationLaunchedRequest();
        message.Action?.Invoke(request);
        ApplicationLaunchedAsync(request);
    }

    [MessageReceiver]
    public void AcceptMessage(AppMessages.AppMessage<IAutoDiscoveryFinishedRequest> message)
    {
        var request = new AutoDiscoveryFinishedRequest();
        message.Action?.Invoke(request);
        AutoDiscoveryFinishedAsync(request);
    }

    [MessageReceiver]
    public void AcceptMessage(AppMessages.AppMessage<IScanStartedRequest> message)
    {
        var request = new ScanStartedRequest();
        message.Action?.Invoke(request);
        ScanStartedAsync(request);
    }

    [MessageReceiver]
    public void AcceptMessage(AppMessages.AppMessage<IScanFinishedRequest> message)
    {
        var request = new ScanFinishedRequest();
        message.Action?.Invoke(request);
        ScanFinishedAsync(request);
    }

    [MessageReceiver]
    public void AcceptMessage(AppMessages.AppMessage<IScanUpdatedRequest> message)
    {
        var request = new ScanUpdatedRequest();
        message.Action?.Invoke(request);
        ScanUpdatedAsync(request);
    }

    [MessageReceiver]
    public void AcceptMessage(AppMessages.AppMessage<IReportGeneratedRequest> message)
    {
        var request = new ReportGeneratedRequest();
        message.Action?.Invoke(request);
        ReportGeneratedAsync(request);
    }

    [MessageReceiver]
    public void AcceptMessage(AppMessages.AppMessage<IReportPrintedOrSavedRequest> message)
    {
        var request = new ReportPrintedOrSavedRequest();
        message.Action?.Invoke(request);
        ReportPrintedOrSavedAsync(request);
    }

    [MessageReceiver]
    public void AcceptMessage(AppMessages.AppMessage<IApplicationClosedRequest> message)
    {
        var request = new ApplicationClosedRequest();
        message.Action?.Invoke(request);
        ApplicationClosedAsync(request);
    }
    #endregion

    public async Task OnAppClosing()
    {
        await Task.Run(() =>
        {
            var request = new ApplicationClosedRequest();
            _client.ApplicationClosed(request);
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
                    if (code == HttpStatusCode.ProxyAuthenticationRequired && _askCredentials)
                    {
                        var proxyCredentials = _proxyCredentialsProvider.GetProxyCredentials();


                        if (proxyCredentials != null)
                        {
                            ProxySettings.Proxy.Credentials = proxyCredentials;
                            continue;
                        }
                        _askCredentials = false;
                    }

                    Thread.Sleep(TimeSpan.FromSeconds(1));

                    LogHelper.Trace(TraceEventType.Error,
                        $"Statistics service warning: {ex.GetType()} {ex.Message}{Environment.NewLine}{ex.StackTrace}");
                }
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
        Action action = () =>
        {
            var folderPath = new DirectoryInfo(System.Reflection.Assembly.GetEntryAssembly().Location).Parent.FullName;
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
        Action action = () => { _client.ScanFinished(request); };
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
        Action action = () => { _client.ReportGenerated(request); };
        ProduceRequest(action);
    }

    public void ReportPrintedOrSavedAsync(IReportPrintedOrSavedRequest request)
    {
        Action action = () => { _client.ReportPrintedOrSaved(request); };
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

        if (malwareDiffCount == 0 && unknownDiffCount == 0) return;


        IScanUpdatedRequest request = new ScanUpdatedRequest()
        {
            CountOfNewMaliciousFilesAfterTheScanWasFinished = malwareDiffCount,
            CountOfNewUnknownFilesAfterTheScanWasFinished = unknownDiffCount
        };

        Action action = () => { _client.ScanUpdated(request); };
        ProduceRequest(action);
    }

}




public enum MessageTypes
{
    StartScan,
    StopScan,
    FilterTarget,
    CMCStatistics
}

public static class AppMessages
{
    public class AppMessage<T>
    {
        public MessageTypes Type { get; private set; }
        public Action<T> Action { get; private set; }

        public AppMessage(MessageTypes type, Action<T> action)
        {
            Type = type;
            Action = action;
        }
    }

    public static class ScanMessages
    {
        public static void Send(MessageTypes type, Action<object> action = null)
        {
            MessageManager.Instance.PublishMessage(new AppMessage<object>(type, action));
        }
    }

    public static class FilterMessages
    {
        public static void Send(Action<SearchUserControl> action = null)
        {
            MessageManager.Instance.PublishMessage(new AppMessage<SearchUserControl>(MessageTypes.FilterTarget, action));
        }
    }

    public static class CMCStatisticsMessages<T> where T : IRequest
    {
        public static void Send(Action<T> request = null)
        {
            MessageManager.Instance.PublishMessage(new AppMessage<T>(MessageTypes.CMCStatistics, request));
        }
    }
}




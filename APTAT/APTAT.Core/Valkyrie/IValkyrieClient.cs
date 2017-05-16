using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Valkyrie.WebApiProvider.Contracts;

namespace APTAT.Core.Valkyrie
{
    public interface IValkyrieClient : IDisposable
    {
        event EventHandler<ValkyrieAnalyzeProgressChangedEventArgs<IValkyrieAnalysisResult>> AnalyzeProgressChanged;
        event EventHandler<AnalyzeCompletedEventArgs> AnalyzeCompleted;
        event EventHandler<ValkyrieAnalyzeStartedEventArgs> AnalyzeStarted;
        void AddFilesToAnalyze(List<IValkyrieFileInfo> filesToAnalyze, bool shouldStartImmediately);
        void AnalyzeAsyncCancel(bool cancelledByUser = true);
        void StartAnalyze(ICamRegistration camRegistration, string login, string password, int machineCount,
            NetworkCredential proxyCredential, string productName);
        string GetValkyrieDetailsLink();
        string GetValkyrieDetailsLink(string apiKey, string scanId);
        void SendEndScanAsync();
        Task SendEndScan();
        void SetBaseUri(Uri uri);
        void AllowSubmit(Dictionary<string, IValkyrieFileInfo> updatedData);
        Task<ValkyrieResponse> SubmitEmailAsync(string[] addresses);
        void InitEmail(string[] addresses);
    }
}

using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Valkyrie.WebApiProvider.Contracts;

namespace APTAT.Valkyrie
{
    public interface IValkyrieClient : IDisposable
    {
        event EventHandler<ValkyrieAnalyzeProgressChangedEventArgs<ValkyrieAnalysisResult>> AnalyzeProgressChanged;
        event EventHandler<AnalyzeCompletedEventArgs<ValkyrieAnalysisResult>> AnalyzeCompleted;
        event EventHandler AnalyzeStarted;
        void AddFilesToAnalyze(List<ValkyrieFileInfo> filesToAnalyze, bool shouldStartImmediately);
        void AnalyzeAsyncCancel();
        void StartAnalyze(CamRegistration camRegistration, List<ValkyrieFileInfo> filesToAnalyze,
            string login, string password, int machineCount, NetworkCredential proxyCredential, string productName);
        string GetValkyrieDetailsLink();
        void SendEndScanAsync();
        Task SendEndScan();
        void SetBaseUri(Uri uri);
        void AllowSubmit(Dictionary<string, ValkyrieFileInfo> updatedData);
        ValkyrieResponse SubmitEmail(string[] addresses);
    }
}

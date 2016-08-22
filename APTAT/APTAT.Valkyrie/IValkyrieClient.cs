using System;
using System.Collections.Generic;
using System.Net;

namespace APTAT.Valkyrie
{
    public interface IValkyrieClient : IDisposable
    {
        event EventHandler<ValkyrieAnalyzeProgressChangedEventArgs<ValkyrieAnalysisResult>> AnalyzeProgressChanged;
        event EventHandler<AnalyzeCompletedEventArgs<ValkyrieAnalysisResult>> AnalyzeCompleted;
        event EventHandler AnalyzeStarted;
        void AnalyzeAsyncCancel();
        void StartAnalyze(CamRegistration camRegistration, List<ValkyrieFileInfo> filesToAnalyze,
            string login, string password, int machineCount, NetworkCredential proxyCredential);
        string GetValkyrieDetailsLink();
    }
}

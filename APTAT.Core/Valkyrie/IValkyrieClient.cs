using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;

namespace APTAT.Core.Valkyrie
{
    public interface IValkyrieClient
    {
        event EventHandler<ProgressChangedEventArgs> AnalyzeProgressChanged;
        event EventHandler<AnalyzeCompletedEventArgs<ValkyrieAnalysisResult>> AnalyzeCompleted;
        event EventHandler AnalyzeStarted;
        void AnalyzeAsyncCancel();
        void StartAnalyze(CamRegistration camRegistration, List<ValkyrieFileInfo> filesToAnalyze,
            string login, string password, int machineCount, NetworkCredential proxyCredential);
        string GetValkyrieDetailsLink();
    }
}

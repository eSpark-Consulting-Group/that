using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Valkyrie.WebApiProvider.Contracts;
using Valkyrie.WebApiProvider.Enums;
using APTAT.Valkyrie;
using APTAT.Core.Storage;
using System.Linq;

namespace APTAT.Core.Valkyrie
{
    internal sealed class ValkyrieClientWithStorage : IValkyrieClientEx
    {
        private readonly IValkyrieClient _valkyrieClient;
        private readonly IValkyrieStorageSubSystem _storage;
        private bool _isValkyrieScanEnded;
        private Guid? _operationUid;
        private int? _valkyrieScanId;
        public ValkyrieClientWithStorage(IValkyrieClient valkyrieClient, IValkyrieStorageSubSystem storageSubSystem)
        {
            _valkyrieClient = valkyrieClient;
            _storage = storageSubSystem;
            _valkyrieClient.AnalyzeCompleted += OnAnalyzeCompleted;
            _valkyrieClient.AnalyzeProgressChanged += OnAnalyzeProgressChanged;
            _valkyrieClient.AnalyzeStarted += OnAnalyzeStarted;
        }

        #region IValkyrieClient

        public event EventHandler<ValkyrieAnalyzeProgressChangedEventArgs<ValkyrieAnalysisResult>> AnalyzeProgressChanged;
        public event EventHandler<AnalyzeCompletedEventArgs> AnalyzeCompleted;
        public event EventHandler<ValkyrieAnalyzeStartedEventArgs> AnalyzeStarted;

        public void AddFilesToAnalyze(List<ValkyrieFileInfo> filesToAnalyze, bool shouldStartImmediately)
        {
            if(_valkyrieScanId.HasValue)
                _storage.SaveValkyrieResults(_valkyrieScanId.Value, 
                    filesToAnalyze.Select(f => new ValkyrieAnalysisResult(f.Sha1, APTAT.Valkyrie.ValkyrieVerdict.InQueue)).ToArray());
            _valkyrieClient.AddFilesToAnalyze(filesToAnalyze, shouldStartImmediately);
        }

        public void AllowSubmit(Dictionary<string, ValkyrieFileInfo> updatedData)
        {
            _valkyrieClient.AllowSubmit(updatedData);
        }

        public void AnalyzeAsyncCancel(bool cancelledByUser = true)
        {
            _valkyrieClient.AnalyzeAsyncCancel(cancelledByUser);
            if (!_isValkyrieScanEnded && _valkyrieScanId.HasValue)
            {
                _storage.TryFinishValkyrieScan(_valkyrieScanId.Value, cancelledByUser);
                _isValkyrieScanEnded = true;
            }
        }

        public void Dispose()
        {
            _valkyrieClient.AnalyzeCompleted -= OnAnalyzeCompleted;
            _valkyrieClient.AnalyzeProgressChanged -= OnAnalyzeProgressChanged;
            _valkyrieClient.AnalyzeStarted -= OnAnalyzeStarted;
            _valkyrieClient.Dispose();
        }


        public string GetValkyrieDetailsLink()
        {
            return _valkyrieClient.GetValkyrieDetailsLink();
        }

        public string GetValkyrieDetailsLink(string apiKey, string scanId)
        {
            return _valkyrieClient.GetValkyrieDetailsLink(apiKey, scanId);
        }

        public Task SendEndScan()
        {
            if (_valkyrieScanId.HasValue)
                _storage.TryFinishValkyrieScan(_valkyrieScanId.Value, false);
            _isValkyrieScanEnded = true;
            return _valkyrieClient.SendEndScan();
        }

        public void SendEndScanAsync()
        {
            if (_valkyrieScanId.HasValue)
                _storage.TryFinishValkyrieScan(_valkyrieScanId.Value, false);
            _isValkyrieScanEnded = true;
            _valkyrieClient.SendEndScanAsync();
        }

        public void SetBaseUri(Uri uri)
        {
            _valkyrieClient.SetBaseUri(uri);
        }
                
        public void StartAnalyze(ICamRegistration camRegistration, string login, string password, int machineCount,
            NetworkCredential proxyCredential, string productName)
        {
            _valkyrieClient.StartAnalyze(camRegistration, login, password, machineCount, proxyCredential, productName);
        }

        public ValkyrieResponse SubmitEmail(string[] addresses)
        {
            var response = _valkyrieClient.SubmitEmail(addresses);
            if (response.Error == null && response.ReturnCode == ErrorCodesEnum.Success)
            {
                _storage.UpdateEmails(addresses);
            }
            return response;
        }



        #endregion
        private void OnAnalyzeCompleted(object sender, AnalyzeCompletedEventArgs e)
        {
            AnalyzeCompleted?.Invoke(sender, e);
        }

        private void OnAnalyzeProgressChanged(object sender, ValkyrieAnalyzeProgressChangedEventArgs<ValkyrieAnalysisResult> e)
        {
            if (_valkyrieScanId.HasValue)
                if (e.IsUpdate)
                    _storage.UpdateValkyrieResult(_valkyrieScanId.Value, e.ProgressInfo.Results);
                else
                    _storage.SaveValkyrieResults(_valkyrieScanId.Value, e.ProgressInfo.Results);
            AnalyzeProgressChanged?.Invoke(sender, e);
        }

        private void OnAnalyzeStarted(object sender, ValkyrieAnalyzeStartedEventArgs e)
        {
            if (_operationUid.HasValue)
                _valkyrieScanId = _storage.SaveValkyrieScan(_operationUid.Value, e.ApiKey, e.ScanId);
            _isValkyrieScanEnded = false;
            AnalyzeStarted?.Invoke(sender, e);
        }
        public void TryFinishUsersScanOnStorage()
        {
            if (_valkyrieScanId.HasValue)
                _storage.TryFinishComputersValkyrieScan(_valkyrieScanId.Value);
        }
        public void StartAnalyze(ICamRegistration camRegistration, string login, string password, int machineCount,
            NetworkCredential proxyCredential, string productName, Guid operationUid)
        {
            _operationUid = operationUid;
            _valkyrieClient.StartAnalyze(camRegistration, login, password, machineCount, proxyCredential, productName);
        }

    }
}

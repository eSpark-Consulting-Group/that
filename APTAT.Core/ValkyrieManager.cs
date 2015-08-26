using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using APTAT.Core.Descent;
using APTAT.Valkyrie.Client;
namespace APTAT.Core
{
    public sealed class ValkyrieManager
    {
        private string _clientId;

        private Valkyrie.Client.ValkyrieClient _client;
   
        public ValkyrieManager(string clientId)
        {

            _clientId = clientId;
            ValkyrieAnalysisResult = new List<FileResult>();
            _client = new Valkyrie.Client.ValkyrieClient(_clientId);
            _client.InProgress += ValkyrieOnProgressChanged;

        }

        public List<FileResult> ValkyrieAnalysisResult { get; set; }

        public void CallValkyrie(ConcurrentDictionary<Guid, ScanResult> scanResults, string login, string password)
        {
            var distinctFileList = GetDistinctFileInfo(scanResults);
            try
            {
                _client.AskValkyrie(distinctFileList, login, password);
            }
            catch (AggregateException ex)
            {
                throw ex;
            }

            ValkyrieAnalysisResult = GetFullResult(_client.AnalysisResults, scanResults);
            OnValkyrieAnalysisCompleted();

        }

        public event EventHandler<InProgressEventArgs> ProgressChanged;
        public event EventHandler Completed;

        private void ValkyrieOnProgressChanged(object sender, InProgressEventArgs e)
        {
            var tmp = ProgressChanged;
            if (tmp != null)
            {
                tmp(this, e);
            }
        }

        private void OnValkyrieAnalysisCompleted()
        {
            var tmp = Completed;
            if (tmp != null)
            {
                tmp(this, EventArgs.Empty);
            }
        }        

        private static List<FileResult> GetFullResult(IEnumerable<AnalysisResult> valkyrieResult, ConcurrentDictionary<Guid, ScanResult> scanResults)
        {
            List<FileResult> fileResults = new List<FileResult>();

            foreach (var scanResult in scanResults)
            {
                foreach (var badFile in scanResult.Value.BadFiles)
                {
                    var found = fileResults.SingleOrDefault(f => f.Sha1 == badFile.Sha1);

                    if (found == null)
                    {
                        found = new FileResult(badFile);
                        fileResults.Add(found);                    

                        var vResult = valkyrieResult.Where(v => v.SHA1 != null).Where(v => v.SHA1.ToUpper() == badFile.Sha1.ToUpper()).FirstOrDefault();

                        if (vResult != null)
                        {
                            found.ValkyrieAnalysisResult = vResult;
                        }
                    }                    

                    if (scanResult.Value.AptReport != null && scanResult.Value.AptReport.Computer != null)
                    {                     
                        var computer = scanResult.Value.AptReport.Computer;
                        if (!found.Computers.Any(c => c.Sid == computer.Sid))
                        {
                            var comp = new ComputerInformation(computer);
                            comp.FilePath = badFile.Path;
                            found.Computers.Add(comp);
                        }
                    }
                }
            }
            return fileResults;
        }

        private List<ValkyrieFileInfo> GetDistinctFileInfo(ConcurrentDictionary<Guid, ScanResult> results)
        {
            Dictionary<string, ScanResult> scanResults = results.Select(r => r.Value).ToDictionary(t => t.IpAddress);

            List<ValkyrieFileInfo> files = new List<ValkyrieFileInfo>();
            foreach (var scanResult in scanResults)
            {
                foreach (var badFile in scanResult.Value.BadFiles)
                {
                    if (!files.Any(f => f.Sha1 == badFile.Sha1))
                    {
                        files.Add(new ValkyrieFileInfo() { Ip = scanResult.Value.IpAddress, Path = badFile.Path, Sha1 = badFile.Sha1 });
                    }
                }
            }
            return files;
        }

    
    }
}

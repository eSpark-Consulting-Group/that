using APTAT.Common;
using UFH.Application.Services;
using System.Linq;

namespace UFH.Application.Models
{
    public interface IScanOperationsManager
    {
        void GetPreviousScans(string keyword);
        bool HasPreviousScans();
        IScanOperationCollection OperationCollection { get; }

        string GetEmail();
    }

    public class ScanOperationsManager : IScanOperationsManager
    {
        private IDataService _dataService;
        private IAptService _aptService;

        public ScanOperationsManager(IDataService dataService, IAptService aptService)
        {
            _dataService = dataService;
            _aptService = aptService;
        }

        public IScanOperationCollection OperationCollection { get; private set; }

        public void GetPreviousScans(string keyword)
        {
            OperationCollection = CompositionHelper.Instance.Resolve<IScanOperationCollection>();

            var scans = _dataService.GetPreviousScans(keyword);
            foreach (var scan in scans)
            {
                if (scan.ValkyrieOperationInfo != null && 
                    !string.IsNullOrEmpty(scan.ValkyrieOperationInfo.ScanId) &&
                    !string.IsNullOrEmpty(scan.ValkyrieOperationInfo.ApiKey))
                    scan.ValkyrieOperationInfo.DetailedLink = _aptService.GetValkyrieDetailsLink(
                        scan.ValkyrieOperationInfo.ApiKey,
                        scan.ValkyrieOperationInfo.ScanId);
                OperationCollection.Add(scan);
            }
        }

        public bool HasPreviousScans()
        {
            return _dataService.HasPreviousScans();
        }

        public string GetEmail()
        {
            return _dataService.GetEmails().Split(',').FirstOrDefault();
        }
    }
}

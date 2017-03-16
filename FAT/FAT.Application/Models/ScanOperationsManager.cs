using FAT.Application.Services;
using System.Linq;
using System.Collections.Generic;

namespace FAT.Application.Models
{
    public interface IScanOperationsManager
    {
        void GetPreviousScans(string keyword);
        bool HasPreviousScans();
        IEnumerable<IScanOperation> ScanOperations { get; }

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

        public IEnumerable<IScanOperation> ScanOperations { get; private set; }

        public void GetPreviousScans(string keyword)
        {
            ScanOperations = _dataService.GetPreviousScans(keyword);
            foreach (var scan in ScanOperations)
            {
                if (scan.ValkyrieOperationInfo != null && !string.IsNullOrEmpty(scan.ValkyrieOperationInfo.ScanId))
                    scan.ValkyrieOperationInfo.DetailedLink = _aptService.GetValkyrieDetailsLink(
                        scan.ValkyrieOperationInfo.ApiKey,
                        scan.ValkyrieOperationInfo.ScanId);
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

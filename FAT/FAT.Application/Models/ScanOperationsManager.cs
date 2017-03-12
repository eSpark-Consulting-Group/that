using FAT.Application.Services;
using System.Linq;
using System.Collections.Generic;

namespace FAT.Application.Models
{
    public interface IScanOperationsManager
    {
        void GetPreviousScans(string keyword);

        IEnumerable<IScanOperation> ScanOperations { get; }

        string GetEmail();
    }

    public class ScanOperationsManager : IScanOperationsManager
    {
        private IDataService _dataService;

        public ScanOperationsManager(IDataService dataService)
        {
            _dataService = dataService;
        }

        public IEnumerable<IScanOperation> ScanOperations { get; private set; }

        public void GetPreviousScans(string keyword)
        {
            ScanOperations = _dataService.GetPreviousScans(keyword);
        }

        public string GetEmail()
        {
            return _dataService.GetEmails().Split(',').FirstOrDefault();
        }
    }
}

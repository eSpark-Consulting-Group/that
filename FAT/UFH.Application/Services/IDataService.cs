using System;
using UFH.Application.Models;
using UFH.Application.Models.Apt;

namespace UFH.Application.Services
{
    public interface IDataService
    {
        IFatScanComputerResult[] GetScanOperationDetailed(Guid operationUid);
        IScanOperation[] GetPreviousScans(string keyword);
        bool HasPreviousScans();
        string GetEmails();

    }
}
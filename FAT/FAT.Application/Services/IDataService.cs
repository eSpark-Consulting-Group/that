using System;
using FAT.Application.Models;
using FAT.Application.Models.Apt;

namespace FAT.Application.Services
{
    public interface IDataService
    {
        IFatScanComputerResult[] GetScanOperationDetailed(Guid operationUid);
        IScanOperation[] GetPreviousScans(string keyword);
        bool HasPreviousScans();
        string GetEmails();

    }
}
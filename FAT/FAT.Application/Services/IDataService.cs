using System;
using APTAT.DAL.Storage.DTO;
using FAT.Application.Models;

namespace FAT.Application.Services
{
    public interface IDataService
    {
        IScanResultItem GetScanOperationDetailed(Guid operationUid);
        IScanOperation[] GetPreviousScans(string keyword);
        string GetEmails();

    }
}
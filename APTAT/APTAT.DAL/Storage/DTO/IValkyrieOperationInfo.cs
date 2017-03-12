using System;

namespace APTAT.DAL.Storage.DTO
{
    public interface IValkyrieOperationInfo
    {
        string ApiKey { get; set; }
        string ScanId { get; set; }
        DateTime? FinishedOnUi { get; set; }
    }
}
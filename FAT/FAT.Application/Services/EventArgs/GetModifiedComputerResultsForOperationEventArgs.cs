using FAT.Application.Models;
using FAT.Application.Services;
using System;

namespace APTAT.Application.Services.EventArgs
{
    public class GetModifiedComputerResultsForOperationEventArgs : DomainEventArgs
    {
        public GetModifiedComputerResultsForOperationEventArgs(ScanResultItem[] computerResults,
            Boolean operationCompleted)
        {
            OperationCompleted = operationCompleted;
            Results = computerResults;
        }

        public Boolean OperationCompleted { get; }

        public ScanResultItem[] Results { get; }
    }
}
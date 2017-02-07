using FAT.Application.Models;
using FAT.Application.Services;
using System;

namespace APTAT.Application.Services.EventArgs
{
    public class GetModifiedComputerResultsForOperationEventArgs : DomainEventArgs
    {
        public GetModifiedComputerResultsForOperationEventArgs(ScanResultItem[] computerResults,
            Boolean operationCompleted, double percentage)
        {
            OperationCompleted = operationCompleted;
            Percentage = percentage;
            Results = computerResults;
        }

        public Boolean OperationCompleted { get; }
        public double Percentage { get; }

        public ScanResultItem[] Results { get; }
    }
}
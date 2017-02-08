using FAT.Application.Models;
using FAT.Application.Services;
using System;
using APTAT.Application.Models.Apt;

namespace APTAT.Application.Services.EventArgs
{
    public class GetModifiedResultsForOperationEventArgs : DomainEventArgs
    {
        public GetModifiedResultsForOperationEventArgs(ScanResultItem[] computerResults,
            FileVerdictContext[] files, Boolean operationCompleted, double percentage)
        {
            OperationCompleted = operationCompleted;
            Percentage = percentage;
            Computers = computerResults;
            Files = files;
        }

        public Boolean OperationCompleted { get; }
        public double Percentage { get; }

        public ScanResultItem[] Computers { get; }
        public FileVerdictContext[] Files { get; }
    }
}
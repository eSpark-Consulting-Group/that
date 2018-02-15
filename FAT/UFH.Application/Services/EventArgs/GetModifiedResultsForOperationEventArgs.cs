using UFH.Application.Models;
using UFH.Application.Services;
using System;
using APTAT.Application.Models.Apt;

namespace APTAT.Application.Services.EventArgs
{
    public class GetModifiedResultsForOperationEventArgs : DomainEventArgs
    {
        public GetModifiedResultsForOperationEventArgs(IScanResultItem[] computerResults,
            IFileVerdictContext[] files, Boolean operationCompleted)
        {
            OperationCompleted = operationCompleted;
            Computers = computerResults;
            Files = files;
        }

        public Boolean OperationCompleted { get; }
        public IScanResultItem[] Computers { get; }
        public IFileVerdictContext[] Files { get; }
    }
}
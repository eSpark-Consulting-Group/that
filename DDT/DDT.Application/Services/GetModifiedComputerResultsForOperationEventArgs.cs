using System;
using DDT.Application.Models;

namespace DDT.Application.Services
{
    public class GetModifiedComputerResultsForOperationEventArgs : DomainEventArgs
    {
        private readonly ComputerDescentResultModel[] _computerResults;
        public GetModifiedComputerResultsForOperationEventArgs(ComputerDescentResultModel[] computerResults,
            Boolean operationCompleted)
        {
            OperationCompleted = operationCompleted;
            _computerResults = computerResults;
        }

        public Boolean OperationCompleted { get; private set; }

        public ComputerDescentResultModel[] Results
        {
            get
            {
                return _computerResults;
            }
        }
    }
}
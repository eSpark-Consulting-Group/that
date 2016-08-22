using System;
using DDT.Application.Models;

namespace DDT.Application.Services
{
    public class GetResultForOperationEventArgs : DomainEventArgs
    {
        private readonly DescentResultModel _result;

        public DescentResultModel Result
        {
            get
            {
                return _result;
            }
        }   
        public GetResultForOperationEventArgs(DescentResultModel result)
        {
            _result = result;
        }
    }
}
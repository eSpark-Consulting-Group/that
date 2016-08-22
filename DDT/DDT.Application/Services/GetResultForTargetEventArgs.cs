using System;
using DDT.Application.Models;

namespace DDT.Application.Services
{
    public class GetResultForTargetEventArgs : DomainEventArgs
    {
        private readonly ComputerDescentResultModel _computerResult;        

        public GetResultForTargetEventArgs(ComputerDescentResultModel computerResult)
        {
            _computerResult = computerResult;
        }

        public ComputerDescentResultModel Result
        {
            get
            {
                return _computerResult;
            }
        }
    }
}
using System;
using System.Collections.Generic;

namespace DDT.Application.Models
{
    public class DescentResultModel
    {
        public Guid OperationUid { get; set; }
        public int TotalTargetsCount { get; set; }
        public bool Completed { get; set; }
        public DateTime? CompletedTime { get; set; }
        public List<ComputerDescentResultModel> ComputerDescentResults { get; set; }
    }
}

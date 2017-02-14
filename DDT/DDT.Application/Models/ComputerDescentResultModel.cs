using APTAT.DeploymentCore;
using APTAT.DeploymentCore.Messages;
using System;
using DDT.Core.Descent;

namespace DDT.Application.Models
{

    public class ComputerDescentResultModel
    {
        public Guid TargetUid { get; set; }
        public IComputer Computer { get; set; }
        public bool Completed { get; set; }
        public double Percentage { get; set; }
        public bool Succeeded { get; set; }
        public FormattedText StatusMessage { get; set; }
        public FormattedText ShortStatusMessage { get; set; }
        public string ErrorCode { get; set; }
        public string SystemMessage { get; set; }
        public int SystemCode { get; set; }
        public TimeSpan ModifiedTime { get; set; }
        public string PackageName { get; set; }
        public bool? Is64 { get; set; }
    }
}

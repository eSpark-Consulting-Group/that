using System;
using System.Linq;
using APTAT.DeploymentCore;
using APTAT.DeploymentCore.Descent;

namespace APTAT.Core.Descent.Copy
{
    public class CopyResult : ComputerDescentResult, ICopyResult
    {
        public string[] FilePathes { get; private set; }
        public CopyResult(IComputer computer, Guid operationUid, ITimeStampProvider timeStampProvider)
            : base(computer, operationUid, timeStampProvider)
        {
        }


        public override void UpdateStatus(IDescentProgressItem progress)
        {
            base.UpdateStatus(progress);
            var item = progress as ICopyDescentProgressItem;
            if (item != null)
            {
                FilePathes = item.FilePathes;
                Modified = TimeStampProvider.GetCurretTimestamp();
            }
        }

        public override void UpdateStatus(IOperationDescentResult result)
        {
            base.UpdateStatus(result);
        }
    }
}

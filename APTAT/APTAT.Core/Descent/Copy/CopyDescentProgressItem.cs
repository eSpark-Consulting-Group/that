using APTAT.DeploymentCore.Descent;
using APTAT.DeploymentCore.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APTAT.Core.Descent.Copy
{
    class CopyDescentProgressItem : DescentProgressItem, ICopyDescentProgressItem
    {
        public string[] FilePathes { get; }
        public CopyDescentProgressItem(Guid computerUid, string hostName, Guid operationUid, FormattedText statusMessage, int percentage,
            string[] filePathes) :
            base (computerUid, hostName, operationUid,statusMessage, percentage, true, percentage == 100) 
        {
            FilePathes = filePathes;
        }
    }
    public interface ICopyDescentProgressItem : IDescentProgressItem
    {
        string[] FilePathes { get; }
    }
}

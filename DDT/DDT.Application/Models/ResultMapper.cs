using System;

using APTAT.DeploymentCore.Descent;
using APTAT.DeploymentCore.Facade;
using APTAT.DeploymentCore.Facade.Descent.DataContracts.DeploymentTargets;
using DDT.Core.Descent;


namespace DDT.Application.Models
{
    public static class ResultMapper
    {
        public static DeploymentItemSource MapFromTargetSource(this DeploymentTargetSource source)
        {
            switch (source)
            {
                case DeploymentTargetSource.Domain:
                    return DeploymentItemSource.Domain;                    
                case DeploymentTargetSource.Workgroup:
                    return DeploymentItemSource.Workgroup;                    
                case DeploymentTargetSource.Ip:
                    return DeploymentItemSource.Ip;                    
                default:
                    throw new NotSupportedException(source.ToString());                    
            }
        }
        public static DeploymentTargetSource ToBusiness(this DeploymentItemSource source)
        {
            switch (source)
            {
                case DeploymentItemSource.Domain:
                    return DeploymentTargetSource.Domain;                    
                case DeploymentItemSource.Workgroup:
                    return DeploymentTargetSource.Workgroup;                    
                case DeploymentItemSource.Ip:
                    return DeploymentTargetSource.Ip;                    
                default:
                    throw new NotSupportedException(source.ToString());
                    
            }
        }

        public static DeploymentTarget[] ToBusiness(this DeploymentItem[] items)
        {
            DeploymentTarget[] result = new DeploymentTarget[items.Length];
            for (Int32 i = 0; i < items.Length; ++i)
            {
                DeploymentItem item = items[i];
                result[i] = new DeploymentTarget
                {
                    Name = item.Name,
                    Uid = item.TargetUid,
                    IPAddress = item.IP ?? "Unknown",                                       
                    IsOnline = item.IsOnline,
                    SystemKind = item.SystemKind,
                    Source = item.Source.ToBusiness(),
                    DnsName = item.DnsName,
                    IsLocalComputer = item.IsAptHost,
                    LdapPath = item.LdapPath

                };
            }
            return result;
        }

        public static DeploymentItem[] MapToDeploymentItems(this DeploymentTarget[] targets)
        {
            DeploymentItem[] result = new DeploymentItem[targets.Length];            
            for (Int32 i = 0; i < targets.Length; ++i)
            {
                DeploymentTarget target = targets[i];
                result[i] = new DeploymentItem
                {
                    Name = target.Name,
                    SystemKind = target.SystemKind,
                    TargetUid = target.Uid,
                    IP = string.IsNullOrEmpty(target.IPAddress) ? "Unknown" : target.IPAddress,
                    Status = target.IsOnline ? "Ready" : "Offline",
                    IsOnline = target.IsOnline,
                    Source =  target.Source.MapFromTargetSource(),
                    DnsName = target.DnsName,
                    IsAptHost = target.IsLocalComputer,
                    LdapPath = target.LdapPath
                };                
            }
            return result;
        }
        public static DescentResultModel MapFromDescentResult(IDescentResult descentResult)
        {
            DescentResultModel model = new DescentResultModel()
            {
                Completed = descentResult.Completed,
                CompletedTime = descentResult.CompletedTime,
                ComputerDescentResults = new System.Collections.Generic.List<ComputerDescentResultModel>()
            };

            foreach (var res in descentResult.ComputerDescentResults)
            {
                model.ComputerDescentResults.Add(MapFromIComputerDescentResult(res));
            }
            model.OperationUid = descentResult.OperationUid;
            model.TotalTargetsCount = descentResult.TotalTargetsCount;
            return model;
        }

        public static ComputerDescentResultModel MapFromIComputerDescentResult(IComputerDescentResult result)
        {
            var packageResult = result as IPackageComputerDescentResult;
            var res = new ComputerDescentResultModel
            {
                Completed = result.Completed,
                Computer = result.Computer,
                ErrorCode = result.ErrorCode,
                Percentage = result.Percentage,
                ShortStatusMessage = result.ShortStatusMessage,
                StatusMessage = result.StatusMessage,
                Succeeded = result.Succeeded,
                SystemCode = result.SystemCode,
                SystemMessage = result.SystemMessage,
                TargetUid = result.TargetUid,
                ModifiedTime = result.Modified,
                Is64 = packageResult?.Is64Bit,
                PackageName = packageResult?.PackageName
            };
            return res;

        }
    }
}

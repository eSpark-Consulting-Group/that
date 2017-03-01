using APTAT.Core.Apt;
using APTAT.Core.Descent.Apt;
using APTAT.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FAT.Application.Models
{
    public static class DalMapper
    {
        public static IScanResultItem[] MapCollection(this IEnumerable<APTAT.DAL.Entities.AptScanResult> aptScanResults)
        {
            List<IScanResultItem> result = new List<IScanResultItem>();
            foreach (var scanRes in aptScanResults)
            {
                result.Add(scanRes.MapItem());
            }
            return result.ToArray();

        }
        public static IScanResultItem MapItem(this APTAT.DAL.Entities.AptScanResult scan)
        {
            ScanResultItem result = new ScanResultItem();
            result.BadFiles = MapBadFiles(scan.Modules);
            result.ScanStage = scan.ScanStage.MapScanStageToBusiness();
            result.StatusMessage = scan.StatusMessage;
            result.TargetUid = scan.TargetUid;
            result.TotalFilesCount = scan.Modules.Count();
            result.UnknownFilesCount = scan.Modules.Where(m => m.IsUnknownOrFailed == true).Count(); //Need to check condition !??
            return result;

        }

        public static ScanStage MapScanStageToBusiness(this APTAT.DAL.Entities.ScanStage stage)
        {
            switch (stage)
            {
                case APTAT.DAL.Entities.ScanStage.Unknown:
                    return ScanStage.Unknown;
                case APTAT.DAL.Entities.ScanStage.Pending:
                    return ScanStage.Pending;
                case APTAT.DAL.Entities.ScanStage.Connecting:
                    return ScanStage.Connecting;
                case APTAT.DAL.Entities.ScanStage.CopyingScanner:
                    return ScanStage.CopyingScanner;
                case APTAT.DAL.Entities.ScanStage.ConfiguringScanner:
                    return ScanStage.ConfiguringScanner;
                case APTAT.DAL.Entities.ScanStage.StartingScanner:
                    return ScanStage.StartingScanner;
                case APTAT.DAL.Entities.ScanStage.ScanInProgress:
                    return ScanStage.ScanInProgress;
                case APTAT.DAL.Entities.ScanStage.ScanCompleted:
                    return ScanStage.ScanCompleted;
                case APTAT.DAL.Entities.ScanStage.ScanFailed:
                    return ScanStage.ScanFailed;
                case APTAT.DAL.Entities.ScanStage.ScanResultsObtaning:
                    return ScanStage.ScanResultsObtaning;
                case APTAT.DAL.Entities.ScanStage.ScanResultsObtaned:
                    return ScanStage.ScanResultsObtaned;
                case APTAT.DAL.Entities.ScanStage.ScanResultsProcessing:
                    return ScanStage.ScanResultsProcessing;
                case APTAT.DAL.Entities.ScanStage.DescentOperationCompleted:
                    return ScanStage.DescentOperationCompleted;
                case APTAT.DAL.Entities.ScanStage.OperationFailed:
                    return ScanStage.OperationFailed;                    
                case APTAT.DAL.Entities.ScanStage.OperationCanceled:
                    return ScanStage.OperationCanceled;
                case APTAT.DAL.Entities.ScanStage.OperationCompleted:
                    return ScanStage.OperationCompleted;
                case APTAT.DAL.Entities.ScanStage.RemoteOperationCompleted:
                    return ScanStage.RemoteOperationCompleted;
                case APTAT.DAL.Entities.ScanStage.OperationCancelling:
                    return ScanStage.OperationCancelling;
                default:
                    throw new ArgumentOutOfRangeException(nameof(stage));
            }

        }

        public static APTAT.DAL.Entities.ScanStage MapScanStageToDal(ScanStage stage)
        {
            switch (stage)
            {
                case ScanStage.Unknown:
                    return APTAT.DAL.Entities.ScanStage.Unknown;                    
                case ScanStage.Pending:
                    return APTAT.DAL.Entities.ScanStage.Pending;
                case ScanStage.Connecting:
                    return APTAT.DAL.Entities.ScanStage.Connecting;
                case ScanStage.CopyingScanner:
                    return APTAT.DAL.Entities.ScanStage.CopyingScanner;
                case ScanStage.ConfiguringScanner:
                    return APTAT.DAL.Entities.ScanStage.ConfiguringScanner;
                case ScanStage.StartingScanner:
                    return APTAT.DAL.Entities.ScanStage.StartingScanner;
                case ScanStage.ScanInProgress:
                    return APTAT.DAL.Entities.ScanStage.ScanInProgress;
                case ScanStage.ScanCompleted:
                    return APTAT.DAL.Entities.ScanStage.ScanCompleted;
                case ScanStage.ScanFailed:
                    return APTAT.DAL.Entities.ScanStage.ScanFailed;
                case ScanStage.ScanResultsObtaning:
                    return APTAT.DAL.Entities.ScanStage.ScanResultsObtaning;
                case ScanStage.ScanResultsObtaned:
                    return APTAT.DAL.Entities.ScanStage.ScanResultsObtaned;
                case ScanStage.ScanResultsProcessing:
                    return APTAT.DAL.Entities.ScanStage.ScanResultsProcessing;
                case ScanStage.DescentOperationCompleted:
                    return APTAT.DAL.Entities.ScanStage.DescentOperationCompleted;
                case ScanStage.OperationFailed:
                    return APTAT.DAL.Entities.ScanStage.OperationFailed;
                case ScanStage.OperationCanceled:
                    return APTAT.DAL.Entities.ScanStage.OperationCanceled;
                case ScanStage.OperationCompleted:
                    return APTAT.DAL.Entities.ScanStage.OperationCompleted;
                case ScanStage.RemoteOperationCompleted:
                    return APTAT.DAL.Entities.ScanStage.RemoteOperationCompleted;
                case ScanStage.OperationCancelling:
                    return APTAT.DAL.Entities.ScanStage.OperationCancelling;
                default:
                    throw new ArgumentOutOfRangeException(nameof(stage));
                    
            }
        }

        private static IBadFileItem[] MapBadFiles(IEnumerable<APTAT.DAL.Entities.Module> modules)
        {
            List<IBadFileItem> result = new List<IBadFileItem>();
            foreach (var file in modules)
            {
                result.Add(MapBadFile(file));
            }
            return result.ToArray();        
        }    

        private static IBadFileItem MapBadFile(APTAT.DAL.Entities.Module file)
        {
            BadFileItem result = new BadFileItem();
            result.Accessed = file.Accessed.GetValueOrDefault();            
            result.Attributes = file.Attributes;
            result.Cloud = file.CloudVerdict.MapFromDal();
            result.Copyright = file.Copyright;
            result.Created = file.Created.GetValueOrDefault();
            result.Description = file.Description;
            result.Modified = file.Modified.GetValueOrDefault();
            result.Product = file.Product;
            result.Sha1 = file.Sha1;
            result.Signer = file.Signer;
            result.Size = file.Size;
            result.Version = file.Version;
            result.Name = file.Name;            
            return result;
        }
    }
}

using APTAT.Core.Storage.Repositories;
using APTAT.Core.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using APTAT.DAL.Storage.DTO;
using APTAT.DeploymentCore.Directory;
using FAT.Application.Models.Apt;

namespace FAT.Application.Models
{
    public static class DalMapper
    {

        public static IFatScanComputerResult[] MapCollection(this IEnumerable<AptScanResultDTO> aptScanResults)
        {
            List<IFatScanComputerResult> result = new List<IFatScanComputerResult>();
            foreach (var scanRes in aptScanResults)
            {
                result.Add(scanRes.MapItem());
            }
            return result.ToArray();
        }

        public static IFatScanComputerResult MapItem(this AptScanResultDTO dto)
        {
            var scan = dto.ScanResult;
            FatScanComputerResult result = new FatScanComputerResult
            {
                BadFiles = MapBadFiles(scan.Modules),
                ScanStage = scan.ScanStage.MapScanStageToBusiness(),
                StatusMessage = scan.StatusMessage,
                TargetUid = scan.TargetUid,
                TotalFilesCount = dto.TotalCount,
                UnknownFilesCount = dto.UnknownsCount,
                MalwareFilesCount = dto.MaliciousCount,
                CleanFilesCount = dto.SafeFilesCount,
                NotAnalyzedFilesCount = dto.NotAnalyzedFilesCount,
            };
            result.InAnalysisFilesCount = result.TotalFilesCount - result.CleanFilesCount
                - result.NotAnalyzedFilesCount - result.MalwareFilesCount - result.UnknownFilesCount;
            return result;
        }

        public static IScanOperation[] MapCollection(this IEnumerable<IAptScanOperationDTO> aptScanResults)
        {
            List<IScanOperation> result = new List<IScanOperation>();
            foreach (var scanRes in aptScanResults)
            {
                result.Add(scanRes.MapItem());
            }
            return result.ToArray();
        }

        public static IScanOperation MapItem(this IAptScanOperationDTO dto)
        {
            ScanOperation result = new ScanOperation()
            {
                FilesCount = dto.FilesCount,
                Finished = dto.Finished,
                ScanMethod = dto.ScanMethod.ToBusiness(),
                MaliciousCount = dto.MaliciousCount,
                UnknownsCount = dto.UnknownsCount,
                Started = dto.Started,
                OperationUid = dto.OperationUid,
                TotalDevicesScanned = dto.TotalDevicesScanned,
                DevicesSourceDescription = new DevicesSourceDescription
                {
                    Description = dto.SourceDescription,
                    Type = dto.Type.ToBusiness()
                },
                ValkyrieOperationInfo = dto.ValkyrieOperationInfo != null ?
                    new ValkyrieOperationInfo(dto.ValkyrieOperationInfo.ApiKey,
                    dto.ValkyrieOperationInfo.ScanId): null
            };
            return result;
        }

        public static ETargetsDiscovererType ToBusiness(this APTAT.DAL.Enums.ETargetsDiscovererType source)
        {
            switch (source)
            {
                case APTAT.DAL.Enums.ETargetsDiscovererType.ActiveDirectory:
                    return ETargetsDiscovererType.ActiveDirectory;

                case APTAT.DAL.Enums.ETargetsDiscovererType.NetworkAddresses:
                    return ETargetsDiscovererType.NetworkAddresses;
                case APTAT.DAL.Enums.ETargetsDiscovererType.Workgroup:
                    return ETargetsDiscovererType.Workgroup;

                default:
                    throw new ArgumentOutOfRangeException(nameof(APTAT.DAL.Enums.ETargetsDiscovererType));
            }
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

        private static IFatFileResult[] MapBadFiles(IEnumerable<APTAT.DAL.Entities.Module> modules)
        {
            return modules.Select(MapBadFile).ToArray();        
        }    

        private static IFatFileResult MapBadFile(APTAT.DAL.Entities.Module file)
        {
            var result = new FatFileResult
            {
                Accessed = file.Accessed.GetValueOrDefault(),
                Attributes = file.Attributes,
                Cloud = file.CloudVerdict.ToBusiness(),
                Copyright = file.Copyright,
                Created = file.Created.GetValueOrDefault(),
                Description = file.Description,
                Modified = file.Modified.GetValueOrDefault(),
                Product = file.Product,
                Sha1 = file.Sha1,
                Signer = file.Signer,
                Size = file.Size,
                Version = file.Version,
                Name = file.Name
            };
            return result;
        }
    }
}

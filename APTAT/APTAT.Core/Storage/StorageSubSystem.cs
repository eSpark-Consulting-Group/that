using System;
using System.Collections.Generic;
using System.Linq;
using APTAT.Core.Apt;
using APTAT.Core.Descent;
using APTAT.Core.Descent.Apt;
using APTAT.DAL.Entities;
using APTAT.DAL.Storage;
using AptScanResult = APTAT.DAL.Entities.AptScanResult;
using APTAT.DAL.Enums;
using APTAT.Valkyrie;
using ValkyrieFileInfo = APTAT.DAL.Entities.ValkyrieFileInfo;

namespace APTAT.Core.Storage
{
    public class StorageSubSystem : IStorageSubSystem, IValkyrieStorageSubSystem
    {
        private readonly Lazy<IEntityStorage> _entityStorage;
        public StorageSubSystem(Lazy<IEntityStorage> entityStorage)
        {
            _entityStorage = entityStorage;
        }
        public void SaveOperationContext(IAptScanOperationContext scanOperationContext)
        {
            var dataObject = new AptScanOperationResult
            {
                OperationUid = scanOperationContext.OperationUid,
                Started = scanOperationContext.Started,
                SourceDescription = scanOperationContext?.DevicesSourceDescription?.Description,
                Source = scanOperationContext?.DevicesSourceDescription.Type.ToDal(),
                ScanMethod = scanOperationContext.ScanMethod.ToData()
            };

            foreach (var aptScanResult in scanOperationContext.ScanResults)
            {
                dataObject.AptScanResults.Add(new AptScanResult
                {
                    IpAddress = aptScanResult.Computer?.IPAddress,
                    ComputerName = aptScanResult.Computer?.Name,
                    OrganizationUnit = aptScanResult.Computer?.OrganizationUnit,
                    OsName = aptScanResult.AptReport?.Computer?.Os.ToString(),
                    LdapPath = aptScanResult.Computer?.LdapPath,
                    IsOnline = aptScanResult.Computer?.IsOnline ?? false,
                    IsLocal = aptScanResult.Computer?.IsLocalComputer ?? false,
                    TargetUid = aptScanResult.TargetUid,
                    Succeeded = aptScanResult.IsCanceled ? false : aptScanResult.Succeeded,
                    Canceled = aptScanResult.IsCanceled,
                    StatusMessage = aptScanResult.ShortStatusMessage.ToString(),
                    TotalCount = aptScanResult.TotalFilesCount
                });
            }
            using (var context = _entityStorage.Value.Context)
            {
                context.AptScanOperationResultRepository.Add(dataObject);
                context.SaveChanges();
            }
        }
        public void UpdateScanResults(IAptScanOperationContext scanOperationContext)
        {
            using (var context = _entityStorage.Value.Context)
            {
                var operation = context.AptScanOperationResultRepository.GetByOperationUid(scanOperationContext.OperationUid);
                if (operation != null)
                    operation.Finished = scanOperationContext.CompletedTime;
                context.SaveChanges();
            }
        }
        public void SaveScanResults(Guid operationUid, IAptScanResult scanResult)
        {
            using (var context = _entityStorage.Value.Context)
            {
                var result = context.AptScanResultRepository.GetByTargetUidAndOperationUid(scanResult.Computer.Uid,
                    operationUid);
                result.Succeeded = scanResult.IsCanceled ? false : scanResult.Succeeded;
                result.StatusMessage = scanResult.ShortStatusMessage.ToString();
                result.OrganizationUnit = scanResult.Computer.OrganizationUnit;                
                result.OsName = scanResult.AptReport?.Computer.Os.ToString();
                result.TargetUid = scanResult.TargetUid;
                result.IpAddress = scanResult.Computer?.IPAddress;
                result.ComputerName = scanResult.Computer?.Name;
                result.IsOnline = scanResult.Computer?.IsOnline ?? false;
                result.IsLocal = scanResult.Computer?.IsLocalComputer ?? false;
                result.Canceled = scanResult.IsCanceled;
                result.LdapPath = scanResult.Computer?.LdapPath;
                result.TotalCount = scanResult.TotalFilesCount;

                foreach (var file in scanResult.Files)
                {
                    if (file.Cloud != Descent.Apt.CloudVerdict.Safe)
                        result.Modules.Add(Mapper.MapModule(file, result.OperationResultId, VerdictSource.RemoteAgent));
                }

                context.SaveChanges();
            }
        }

        public void UpdateVerdictForOperation(Guid operationUid, Dictionary<string, Descent.Apt.CloudVerdict> resultsWithoutErrors,
            VerdictSource verdictSource)
        {
            var group = resultsWithoutErrors.GroupBy(kvp => kvp.Value.ToData(), kvp => kvp.Key).ToArray();
            using (var context = _entityStorage.Value.Context)
            {
                context.ModulesRepository.UpdateVerdictByHashAndOperationUid(operationUid,group, verdictSource);
            }
        }

        public int SaveValkyrieScan(Guid operationUid, string apiKey, string scanId)
        {
            ValkyrieScanInfo scanInfo;
            using (var context = _entityStorage.Value.Context)
            {
                var operation = context.AptScanOperationResultRepository.GetByOperationUid(operationUid);
                scanInfo = new ValkyrieScanInfo
                {
                    ApiKey = apiKey,
                    ScanId = scanId,
                    AptScanOperationResult = operation,
                    Started = DateTime.UtcNow 
                };
                context.ValkyrieScansRepository.Add(scanInfo);
                context.SaveChanges();
            }
            return scanInfo.AptScanOperationResultId;
        }

        public void TryFinishValkyrieScan(int scanId, bool isCancelled)
        {
            using (var context = _entityStorage.Value.Context)
            {
                var scanInfo = context.ValkyrieScansRepository.ExecuteFind(new object[] { scanId});
                if (scanInfo.FinishedWithoutErrorHandling == null)
                    scanInfo.FinishedWithoutErrorHandling = DateTime.UtcNow;
                if (scanInfo.Finished == null)
                {
                    scanInfo.Finished = DateTime.UtcNow;
                    scanInfo.IsCancelled = isCancelled;
                }
                context.SaveChanges();
            }
        }

        public void TryFinishComputersValkyrieScan(int scanId)
        {
            using (var context = _entityStorage.Value.Context)
            {
                var scanInfo = context.ValkyrieScansRepository.ExecuteFind(new object[] { scanId });
                if (scanInfo.FinishedWithoutErrorHandling == null)
                    scanInfo.FinishedWithoutErrorHandling = DateTime.UtcNow;
                context.SaveChanges();
            }
        }

        public void FailValkyrieScan(int scanId, bool isCancelled)
        {

        }

        public void SaveValkyrieResults(int scanId, IValkyrieAnalysisResult[] valkyrieResults)
        {
            List<ValkyrieFileInfo> files = new List<ValkyrieFileInfo>(valkyrieResults.Length);
            foreach (var valkyrieResult in valkyrieResults)
            {
                files.Add(new ValkyrieFileInfo
                {
                    ValkyrieScanInfoId = scanId,
                    Sha1 = valkyrieResult.Sha1,
                    IsWhiteListed = valkyrieResult.IsWhiteListed,
                    Verdict = (DAL.Entities.ValkyrieVerdict)valkyrieResult.Verdict,
                    MalwareProbability = valkyrieResult.MalwareProbability,
                    OperationStatus = valkyrieResult.OperationStatus,
                    SkipReason = valkyrieResult.SkipReason,
                    IsSubmitted = valkyrieResult.IsSubmitted,
                    AnalyzeId = valkyrieResult.AnalyzeId
                });
            }
            using (var context = _entityStorage.Value.Context)
            {
                context.ValkyrieFilesRepository.AddMany(files);
                context.SaveChanges();
            }
        }

        public void SaveOrUpdateValkyrieResult(int scanId, IValkyrieAnalysisResult[] valkyrieResults)
        {
            List<ValkyrieFileInfo> files = new List<ValkyrieFileInfo>(valkyrieResults.Length);
            foreach (var valkyrieResult in valkyrieResults)
            {
                files.Add(new ValkyrieFileInfo
                {
                    ValkyrieScanInfoId = scanId,
                    Sha1 = valkyrieResult.Sha1,
                    IsWhiteListed = valkyrieResult.IsWhiteListed,
                    Verdict = (DAL.Entities.ValkyrieVerdict)valkyrieResult.Verdict,
                    MalwareProbability = valkyrieResult.MalwareProbability,
                    OperationStatus = valkyrieResult.OperationStatus,
                    SkipReason = valkyrieResult.SkipReason,
                    IsSubmitted = valkyrieResult.IsSubmitted,
                    AnalyzeId = valkyrieResult.AnalyzeId
                });
            }
            using (var context = _entityStorage.Value.Context)
            {
                foreach (var file in files)
                    context.ValkyrieFilesRepository.AddOrUpdate(file);
                context.SaveChanges();
            }
        }

        public void UpdateEmails(string[] newEmails)
        {
            using (var context = _entityStorage.Value.Context)
            {
                var oldEmails=context.EmailRepository.EntityQuery.ToList();
                foreach (var email in oldEmails)
                {
                    context.EmailRepository.Remove(email);
                    context.SaveChanges();
                }

                foreach (var email in newEmails)
                {
                    context.EmailRepository.Add(new Email { Address = email });
                    context.SaveChanges();
                }
            }
        }



    }
}

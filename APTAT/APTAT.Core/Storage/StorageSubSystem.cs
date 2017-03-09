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


namespace APTAT.Core.Storage
{
    public class StorageSubSystem : IStorageSubSystem
    {
        private readonly IEntityStorage _entityStorage;
        public StorageSubSystem(IEntityStorage storage)
        {
            _entityStorage = storage;
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
                    TargetUid = aptScanResult.TargetUid,
                    Succeeded = aptScanResult.Succeeded,
                    StatusMessage = aptScanResult.ShortStatusMessage.ToString()
                });
            }
            using (var context = _entityStorage.GetContext())
            {
                context.AptScanOperationResultRepository.Add(dataObject);
                context.SaveChanges();
            }
        }
        public void UpdateScanResults(IAptScanOperationContext scanOperationContext)
        {
            using (var context = _entityStorage.GetContext())
            {
                var operation = context.AptScanOperationResultRepository.GetByOperationUid(scanOperationContext.OperationUid);
                operation.Finished = scanOperationContext.CompletedTime;
                context.SaveChanges();
            }
        }
        public void SaveScanResults(Guid operationUid, IAptScanResult scanResult)
        {
            using (var context = _entityStorage.GetContext())
            {
                var result = context.AptScanResultRepository.GetByTargetUidAndOperationUid(scanResult.Computer.Uid,
                    operationUid);
                result.Succeeded = scanResult.Succeeded;
                result.StatusMessage = scanResult.ShortStatusMessage.ToString();
                result.OrganizationUnit = scanResult.Computer.OrganizationUnit;                
                result.OsName = scanResult.AptReport?.Computer.Os.ToString();
                result.TargetUid = scanResult.TargetUid;

                foreach (var file in scanResult.Files)
                {
                    result.Modules.Add(Mapper.MapModule(file, VerdictSource.RemoteAgent));
                }
                context.SaveChanges();
            }
        }

        public void UpdateVerdictForOperation(Guid operationUid, Dictionary<string, Descent.Apt.CloudVerdict> resultsWithoutErrors,
            VerdictSource verdictSource)
        {
            var group = resultsWithoutErrors.GroupBy(kvp => kvp.Value.ToData(), kvp => kvp.Key).ToArray();
            using (var context = _entityStorage.GetContext())
            {
                context.ModulesRepository.UpdateVerdictByHashAndOperationUid(operationUid,group, verdictSource);
            }
        }

        public int SaveValkyrieScan(Guid operationUid, string apiKey, string scanId)
        {
            ValkyrieScanInfo scanInfo;
            using (var context = _entityStorage.GetContext())
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
            using (var context = _entityStorage.GetContext())
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
            using (var context = _entityStorage.GetContext())
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

        public void SaveValkyrieResults(int scanId, Dictionary<string, FileResult>.ValueCollection valkyrieResults)
        {
            List<ValkyrieFileInfo> files = new List<ValkyrieFileInfo>(valkyrieResults.Count);
            foreach (var valkyrieResult in valkyrieResults)
            {
                files.Add(new ValkyrieFileInfo
                {
                    ValkyrieScanInfoId = scanId,
                    Sha1 = valkyrieResult.Sha1,
                    IsWhiteListed = valkyrieResult.ValkyrieAnalysisResult.IsWhiteListed,
                    Verdict = (DAL.Entities.ValkyrieVerdict)valkyrieResult.ValkyrieAnalysisResult.Verdict,
                    MalwareProbability = valkyrieResult.ValkyrieAnalysisResult.MalwareProbability,
                    OperationStatus = valkyrieResult.ValkyrieAnalysisResult.OperationStatus,
                    SkipReason = valkyrieResult.ValkyrieAnalysisResult.SkipReason,
                    IsSubmitted = valkyrieResult.ValkyrieAnalysisResult.IsSubmitted,
                    AnalyzeId = valkyrieResult.ValkyrieAnalysisResult.AnalyzeId
                });
            }
            using (var context = _entityStorage.GetContext())
            {
                context.ValkyrieFilesRepository.AddMany(files);
                context.SaveChanges();
            }
        }

        public void UpdateValkyrieResult(int scanId, Dictionary<string, FileResult>.ValueCollection valkyrieResults)
        {
            List<ValkyrieFileInfo> files = new List<ValkyrieFileInfo>(valkyrieResults.Count);
            foreach (var valkyrieResult in valkyrieResults)
            {
                files.Add(new ValkyrieFileInfo
                {
                    ValkyrieScanInfoId = scanId,
                    Sha1 = valkyrieResult.Sha1,
                    IsWhiteListed = valkyrieResult.ValkyrieAnalysisResult.IsWhiteListed,
                    Verdict = (ValkyrieVerdict)valkyrieResult.ValkyrieAnalysisResult.Verdict,
                    MalwareProbability = valkyrieResult.ValkyrieAnalysisResult.MalwareProbability,
                    OperationStatus = valkyrieResult.ValkyrieAnalysisResult.OperationStatus,
                    SkipReason = valkyrieResult.ValkyrieAnalysisResult.SkipReason,
                    IsSubmitted = valkyrieResult.ValkyrieAnalysisResult.IsSubmitted,
                    AnalyzeId = valkyrieResult.ValkyrieAnalysisResult.AnalyzeId
                });
            }
            using (var context = _entityStorage.GetContext())
            {
                foreach (var file in files)
                    context.ValkyrieFilesRepository.AddOrUpdate(file);
                context.SaveChanges();
            }
        }

        public void UpdateEmails(string[] newEmails)
        {
            using (var context = _entityStorage.GetContext())
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

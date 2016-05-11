using System;
using System.Diagnostics;
using System.Linq;
using APTAT.Core.Descent;

namespace APTAT.Core.Reports
{
    internal class AptReportBuilder : IAptReportBuilder
    {
        private readonly IDescentSubSystem _descentSubSystem;
        private readonly ReportBuilder _reportBuilder = new ReportBuilder();

        public AptReportBuilder(IDescentSubSystem descentSubSystem, AptAtManager valkyrie)
        {
            _descentSubSystem = descentSubSystem;
            _valkyrieSubSystem = valkyrie;
        }

        /* FAKE DATA
        public void BeginBuildReport(Guid operationUid, ReportType reportType, object state = null)
        {
            //data = GetFakeData();
            var results = _descentSubSystem.GetScanResultsByOperationUid(operationUid);
            // if (results.Finished == DateTime.MinValue)
            //     throw new DataNotReadyException();
            BeginBuildReport(results, reportType, state);
        }

        public void BeginBuildReport(AptScanOperationContext operationContext, ReportType reportType, object state = null)
        {

            AptReportData data = GetFakeData();

            ReportBuilder.BuildReportDelegate action = new ReportBuilder.BuildReportDelegate(_reportBuilder.Build);
            Context context = new Context() { Action = action, State = state };
            action.BeginInvoke(data, reportType, BuildReportCallback, context);

        }
        */
        public void BeginBuildReport(Guid operationUid, ReportType reportType, object state = null)
        {
            //data = GetFakeData();

            var results = _descentSubSystem.GetScanResultsByOperationUid(operationUid);
            if (results.Finished == DateTime.MinValue)
                throw new DataNotReadyException();

            BeginBuildReport(results, reportType, state);
        }

        public void BeginBuildReport(AptScanOperationContext operationContext, ReportType reportType, object state = null)
        {
            if (operationContext != null)
            {
                AptReportData data = new AptReportData
                {
                    OperationUid = operationContext.OperationUid,
                    ScanStarted = operationContext.Started.ToLocalTime(),
                    ScanFinished = operationContext.Finished.ToLocalTime(),
                    TargetsSource = operationContext.TargetsSourceName,
                    TargetsFailed = operationContext.FailedTargetsCount,
                    TargetsNotPerformed = operationContext.NotPerformedTargetsCount,
                    TotalTargetsCount = operationContext.TotalTargetsCount,
                    TargetsSucceeded = operationContext.SucceededTargetsCount,
                    Targets = operationContext.ScanResults.Values.ToArray(),
                    DevicesSourceDescription = operationContext.DevicesSourceDescription,
                    ValkyrieAnalysisResult = _valkyrieSubSystem.ValkyrieAnalysisResult

                };

                ReportBuilder.BuildReportDelegate action = new ReportBuilder.BuildReportDelegate(_reportBuilder.Build);
                Context context = new Context() { Action = action, State = state };
                action.BeginInvoke(data, reportType, BuildReportCallback, context);
            }
            else
            {
                throw new InvalidOperationException("Scan results not found.");
            }
        }

        //        private static AptReportData GetFakeData()
        //        {
        //            AptReportData data = new AptReportData();
        //            data.ScanFinished = DateTime.Now;
        //            data.ScanStarted = DateTime.Now.AddHours(-1);
        //            data.TargetsFailed = 1;
        //            data.TotalTargetsCount = 50;
        //            data.TargetsSource = "dev.comodo.od.ua";
        //            data.TargetsSucceeded = 49;
        //
        //            var unknown = new Module() { Path = @"C:\Program Files (x86)\COMODO\Comodo Internet Security\cfp.exe", Accessed = DateTime.Now, Attributes = "", Cloud = CloudVerdict.Unknown, Copyright = "Microsoft", Created = DateTime.Now.AddYears(-1), Description = "blah-blah-blah", Id = "1", Modified = DateTime.Now, Product = "Windows", Sha1 = "432532907249321", Signer = "Microsoft", Size = 300, Version = "1.0.0.0" };
        //            var malware = new Module() { Path = @"C:\Windows\svch0st.exe", Accessed = DateTime.Now, Attributes = "", Cloud = CloudVerdict.Malware, Copyright = "N/A", Created = DateTime.Now.AddYears(-2), Description = "blah-blah-blah", Id = "2", Modified = DateTime.Now, Product = "Windows", Sha1 = "432532907249322", Signer = "Microsoft", Size = 777, Version = "1.0.0.0" };
        //            var svchost = new Module() { Path = @"D:\svch0st.exe", Accessed = DateTime.Now, Attributes = "", Cloud = CloudVerdict.Malware, Copyright = "N/A", Created = DateTime.Now.AddYears(-3), Description = "blah-blah-blah", Id = "4", Modified = DateTime.Now, Product = "Windows", Sha1 = "432532907249322", Signer = "Microsoft", Size = 777, Version = "1.0.0.0" };
        //            var malware2 = new Module() { Path = @"C:\Windows\expl0rer.exe", Accessed = DateTime.Now, Attributes = "", Cloud = CloudVerdict.Malware, Copyright = "N/A", Created = DateTime.Now.AddYears(-5), Description = "blah-blah-blah", Id = "3", Modified = DateTime.Now, Product = "Windows", Sha1 = "432532907249323", Signer = "Microsoft", Size = 777, Version = "1.0.0.0" };
        //
        //            ScanResult result = new ScanResult() { HostName = "agubernskyy-new", IpAddress = "10.100.65.223", OrganizationUnit = "Dev", MalwareFilesCount = 3, UnknownFilesCount = 1, SafeFilesCount = 303, TotalFilesCount = 307, ResultType = ScanResultType.Completed, TargetUid = new Guid("{D0221479-370A-4429-AB3A-FAE96D11AD02}") };
        //            result.SetAptReportInstance(new AptReport() { Modules = new Module[] { /*new Module() { Path = "kernel32.dll", Cloud = CloudVerdict.Unknown },*/ unknown, malware, svchost, malware2 } });
        //            ScanResult result2 = new ScanResult() { HostName = "dkolomiyets", OrganizationUnit = "Dev", IpAddress = "10.100.65.3", MalwareFilesCount = 2, UnknownFilesCount = 1, SafeFilesCount = 337, TotalFilesCount = 340, ResultType = ScanResultType.Completed, TargetUid = new Guid("{911DD54A-9714-4718-8DCC-2AA15A18E6A8}") };
        //            result2.SetAptReportInstance(new AptReport() { Modules = new Module[] { malware, unknown, malware2 } });
        //            ScanResult result3 = new ScanResult() { HostName = "cesmuser", OrganizationUnit = "QA", IpAddress = "10.100.65.225", MalwareFilesCount = 0, UnknownFilesCount = 0, SafeFilesCount = 100, TotalFilesCount = 100, ResultType = ScanResultType.Completed, TargetUid = new Guid("{09B449D0-366B-47CF-A6CD-8EF15DAF9B9F}") };
        //            result3.SetAptReportInstance(new AptReport() { Modules = new Module[] { /*new Module() { Path = "user32.dll", Cloud = CloudVerdict.Safe }*/ } });
        //
        //            data.Targets = new ScanResult[] { result, result2, result3 };
        //            return data;
        //        }

        private void BuildReportCallback(IAsyncResult ar)
        {
            Context context = ar.AsyncState as Context;
            if (context == null)
                return;

            ReportCompletedEventArgs args = new ReportCompletedEventArgs();
            try
            {
                string fileName = context.Action.EndInvoke(ar);
                args.FileName = fileName;
                args.State = context.State;
            }
            catch (Exception ex)
            {
                LogHelper.Trace(TraceEventType.Error, TraceCategory.Reports, "Report failed: {0}", ex);
                args.Error = ex;
            }
            finally
            {
                OnReportCompleted(args);
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Optimized);
            }
        }

        public event EventHandler<ReportCompletedEventArgs> ReportCompleted;
        private AptAtManager _valkyrieSubSystem;

        protected virtual void OnReportCompleted(ReportCompletedEventArgs args)
        {
            EventHandler<ReportCompletedEventArgs> temp = ReportCompleted;
            if (temp != null)
                temp(this, args);
        }

        internal class Context
        {
            internal ReportBuilder.BuildReportDelegate Action { get; set; }
            internal object State { get; set; }
        }
    }
}

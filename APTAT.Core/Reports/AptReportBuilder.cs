using System;
using System.Diagnostics;
using System.Linq;
using APTAT.Core.Descent;
using APTAT.Core.Descent.Apt;

namespace APTAT.Core.Reports
{
    internal class AptReportBuilder : IAptReportBuilder
    {
        private readonly IDescentSubSystem _descentSubSystem;
        private readonly ReportBuilder _reportBuilder = new ReportBuilder();
        private readonly AptAtManager _valkyrieSubSystem;

        public AptReportBuilder(IDescentSubSystem descentSubSystem, AptAtManager valkyrie)
        {
            _descentSubSystem = descentSubSystem;
            _valkyrieSubSystem = valkyrie;
        }

        
        public void BeginBuildReport(Guid operationUid, ReportType reportType, object state = null)
        {

            var results = _descentSubSystem.GetAptScanResultsByOperationUid(operationUid);
            if (!results.CompletedTime.HasValue || results.CompletedTime.Value == DateTime.MinValue)
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
                    ScanFinished = operationContext.CompletedTime == null? DateTime.MinValue : 
                        operationContext.CompletedTime.Value.ToLocalTime(),
                    TargetsFailed = operationContext.FailedTargetsCount,
                    TargetsNotPerformed = operationContext.NotPerformedTargetsCount,
                    TotalTargetsCount = operationContext.TotalTargetsCount,
                    TargetsSucceeded = operationContext.SucceededTargetsCount,
                    Targets = operationContext.ScanResults.Values.ToArray(),
                    ValkyrieAnalysisResult = _valkyrieSubSystem.ValkyrieAnalysisResult,
                    ValkyrieOperationUid = _valkyrieSubSystem.ValkyrieOperationUid,
                    DevicesSourceDescription = operationContext.DevicesSourceDescription
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

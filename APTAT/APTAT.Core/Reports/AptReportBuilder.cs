using System;
using System.Diagnostics;
using System.Linq;
using APTAT.Common;

namespace APTAT.Core.Reports
{
    internal class AptReportBuilder : IAptReportBuilder
    {
        private readonly ReportBuilder _reportBuilder = new ReportBuilder();
        private readonly IAptAtManagerInternal _aptAtManager;

        public AptReportBuilder(IAptAtManagerInternal aptAtManager)
        {
            _aptAtManager = aptAtManager;
        }

        public void BeginLastBuildReport(ReportType reportType, object state = null)
        {
            var operationContext = _aptAtManager.LastAptScanOperationContext;
            if (operationContext != null)
            {
                if (!operationContext.CompletedTime.HasValue || operationContext.CompletedTime.Value == DateTime.MinValue)
                    throw new DataNotReadyException();
                AptReportData data = new AptReportData
                {
                    OperationUid = operationContext.OperationUid,
                    ScanStarted = operationContext.Started.ToLocalTime(),
                    ScanFinished = (DateTime) operationContext.CompletedTime?.ToLocalTime(),
                    TargetsFailed = operationContext.FailedTargetsCount,
                    TargetsNotPerformed = operationContext.NotPerformedTargetsCount,
                    TotalTargetsCount = operationContext.TotalTargetsCount,
                    TargetsSucceeded = operationContext.SucceededTargetsCount,
                    Targets = operationContext.ScanResults.ToArray(),
                    ValkyrieAnalysisResult = _aptAtManager.ValkyrieAnalysisResult
                        .ToDictionary(k => k.Key, k => k.Value, StringComparer.OrdinalIgnoreCase),
                    ValkyrieOperationUid = _aptAtManager.ValkyrieOperationUid,
                    FlsLookupOperationUid = _aptAtManager.FlsLookupOperationUid,
                    DevicesSourceDescription = operationContext.DevicesSourceDescription
                };

                ReportBuilder.BuildReportDelegate action = _reportBuilder.Build;
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
            ReportCompleted?.Invoke(this, args);
        }

        internal class Context
        {
            internal ReportBuilder.BuildReportDelegate Action { get; set; }
            internal object State { get; set; }
        }
    }
}

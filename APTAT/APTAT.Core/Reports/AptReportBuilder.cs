using System;
using System.Diagnostics;
using System.Linq;
using APTAT.Common;

namespace APTAT.Core.Reports
{
  
    internal class AptReportBuilder : IAptReportBuilderInternal
    {
        private readonly IReportBuilder _reportBuilder;
        private readonly IResultMergeHelper _mergeHelper;
        private IAptAtManagerInternal _aptAtManager;

        public AptReportBuilder(IReportBuilder reportBuilder, IResultMergeHelper mergeHelper)
        {
            _reportBuilder = reportBuilder;
            _mergeHelper = mergeHelper;
        }

        public void Init(IAptAtManagerInternal aptAtManager)
        {
            _aptAtManager = aptAtManager;
        }

        public void BeginLastBuildReport(ReportType reportType,  bool saveFile = true, object state = null)
        {
            var operationContext = _aptAtManager.AggregatedScanResults;
            if (operationContext != null)
            {
                if (!operationContext.CompletedTime.HasValue || operationContext.CompletedTime.Value == DateTime.MinValue)
                    throw new DataNotReadyException();

                AptReportData data = _reportBuilder.CreateReportData(_mergeHelper, operationContext, _aptAtManager.ValkyrieAnalysisResult, _aptAtManager.ValkyrieOperationUid, _aptAtManager.FlsLookupOperationUid);

                data.SetTargets(operationContext.ScanResults.ToArray());
                data.UpdateAllBadFilesSorted();
                ReportBuilder.BuildReportDelegate action = _reportBuilder.Build;
                Context context = new Context { Action = action, State = state };
                action.BeginInvoke(data, reportType, saveFile, BuildReportCallback, context);
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
                ReportFile reportFile = context.Action.EndInvoke(ar);
                args.DocumentBytes = reportFile.Data;
                args.FileName = reportFile.FileName;
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

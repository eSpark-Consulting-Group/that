using System;

namespace APTAT.Core.Reports
{
    public enum ReportScanStage
    {
        FlsScan,
        ValkyrieScan
    }

    public static class ReportTypeExt
    {
        public static ReportScanStage ScanStage(this ReportType reportType)
        {
            switch (reportType)
            {
                case ReportType.Executive:             
                case ReportType.DetailedPerDevice:             
                case ReportType.DetailedPerProgram:
                    return Reports.ReportScanStage.FlsScan;
                    
                case ReportType.ExecutiveWithValkyrie:
                case ReportType.DetailedPerDeviceWithValkyrie:                    
                case ReportType.DetailedPerProgramWithValkyrie:
                    return Reports.ReportScanStage.ValkyrieScan;

                default:
                    throw new InvalidOperationException(nameof(reportType));
                    
            }

        }
    }

    public enum ReportType
    {
        Executive,
        DetailedPerDevice,
        DetailedPerProgram,
        ExecutiveWithValkyrie,
        DetailedPerDeviceWithValkyrie,
        DetailedPerProgramWithValkyrie,

    }
}

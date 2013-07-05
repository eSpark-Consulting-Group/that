using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.Reporting.WinForms;

namespace APTAT.Core.Reports
{
    public class ReportBuilder
    {
        private const string ExecutiveReportName = "ExecutiveAptReport";
        private const string DetailedPerDeviceReportName = "DetailedPerDeviceAptReport";
        private const string DetailedPerProgramReportName = "DetailedPerProgramAptReport";
        private string _fileName;

        public delegate string BuildReportDelegate(AptReportData data, ReportType reportType);

        public string Build(AptReportData data, ReportType reportType)
        {
            if (data == null)
                return null;

            _fileName = GetFileName(data.OperationUid, reportType);
            if (File.Exists(_fileName))
                return _fileName;

            switch (reportType)
            {
                case ReportType.Executive:
                    BuildExecutiveReport(_fileName, data);
                    break;
                case ReportType.DetailedPerDevice:
                    BuildDetailedPerDeviceReport(_fileName, data);
                    break;
                case ReportType.DetailedPerProgram:
                    BuildDetailedPerProgramReport(_fileName, data);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("reportType");
            }

            return _fileName;
        }

        private static void BuildExecutiveReport(string fileName, AptReportData data)
        {
            ReportDataSource fileRatingDataSource = new ReportDataSource("FileRatingDataSet", data.GetFileRatingCollection());
            ReportDataSource devicesRatingDataSource = new ReportDataSource("DevicesRatingDataSet", data.GetDevicesRatingCollection());

            GenerateExecutiveReportFile(fileName, "AptReportDataSet", new AptReportData[] { data }, "APTAT.Core.Reports.Executive.rdlc", null, fileRatingDataSource, devicesRatingDataSource);
        }

        private static void BuildDetailedPerDeviceReport(string fileName, AptReportData data)
        {
            ReportDataSource allDevicesDataSource = new ReportDataSource("ScanResultDataSet", data.SucceededTargets);
            ReportDataSource vulnerableDevicesExDataSource = new ReportDataSource("VulnerableDeviceExDataSet", data.GetTop10VulnerableDevices());
            SubReportContext unknownFilesSubReportContext = new SubReportContext() { Data = data.SucceededTargets, DataSourceName = "ModuleDataSet", FileName = "APTAT.Core.Reports.UnknownFiles.rdlc", ReportName = "UnknownFiles" };
            SubReportContext maliciousFilesSubReportContext = new SubReportContext() { Data = data.SucceededTargets, DataSourceName = "ModuleDataSet", FileName = "APTAT.Core.Reports.MaliciousFiles.rdlc", ReportName = "MaliciousFiles" };

            GenerateDetailedPerDevicesReportFile(fileName, "AptReportDataSet", new AptReportData[] { data }, "APTAT.Core.Reports.DetailedDevices.rdlc", null, new ReportDataSource[] { allDevicesDataSource, vulnerableDevicesExDataSource }, new SubReportContext[] { unknownFilesSubReportContext, maliciousFilesSubReportContext });
        }

        private static void BuildDetailedPerProgramReport(string fileName, AptReportData data)
        {
            var allBadFilesData = data.GetAllBadFilesSorted();
            ReportDataSource allBadFilesDataSource = new ReportDataSource("DetailedBadFileDataSet", allBadFilesData);
            ReportDataSource mostWantedProgramsExDataSource = new ReportDataSource("MostWantedBadFileDataSet", data.GetTop10PopularBadFiles());
            SubReportContext devicesSubReportDataSeource = new SubReportContext() { Data = allBadFilesData, DataSourceName = "ScannedTargetDataSet", FileName = "APTAT.Core.Reports.Devices.rdlc", ReportName = "Devices" };

            GenerateDetailedPerProgramReportFile(fileName, "AptReportDataSet", new AptReportData[] { data }, "APTAT.Core.Reports.DetailedPrograms.rdlc", null, new ReportDataSource[] { allBadFilesDataSource, mostWantedProgramsExDataSource }, new SubReportContext[] { devicesSubReportDataSeource });
        }

        private static void GenerateExecutiveReportFile(string path, string dataSourceName, object data, string reportSourceFile, IEnumerable<ReportParameter> parameters, params ReportDataSource[] additionDataSource)
        {
            using (LocalReport localReport = new LocalReport())
            {
                Stream reportContentStream =
                    Assembly.GetExecutingAssembly().GetManifestResourceStream(reportSourceFile);
                localReport.LoadReportDefinition(reportContentStream);

                //ReportParameter languageParam = new Microsoft.Reporting.WinForms.ReportParameter("Language", cultureInfo.Name);
                //ReportParameter dateCreated = new Microsoft.Reporting.WinForms.ReportParameter("DateCreated", TimeZoneInfo.ConvertTime(DateTime.UtcNow, timeZone).ToString());
                ReportParameter reportDateParam = new ReportParameter("ReportDate", DateTime.Now.ToString());
                localReport.SetParameters(new[] {reportDateParam});

                if (parameters != null && parameters.Count() > 0)
                    localReport.SetParameters(parameters);

                localReport.DataSources.Add(new ReportDataSource(dataSourceName, data));
                if (additionDataSource != null)
                {
                    foreach (var reportDataSource in additionDataSource)
                    {
                        localReport.DataSources.Add(reportDataSource);
                    }
                }


                ExportToFile(path, localReport);
            }
        }

        private static void GenerateDetailedPerDevicesReportFile(string path, string dataSourceName, object data, string reportSourceFile, IEnumerable<ReportParameter> parameters, IEnumerable<ReportDataSource> additionDataSource, IEnumerable<SubReportContext> subReports)
        {
            using (LocalReport localReport = new LocalReport())
            {

                //===========================================================================
                SubreportProcessingEventHandler subReportProcessingDelegate = null;
                subReportProcessingDelegate = delegate(object sender, SubreportProcessingEventArgs e)
                {
                    string computerKey = e.Parameters[0].Values[0];
                    Guid uid = new Guid(computerKey);

                    if (e.ReportPath == "UnknownFiles")
                    {
                        var context = (from subReport in subReports where subReport.ReportName == e.ReportPath select subReport).FirstOrDefault();
                        if (context != null)
                        {
                            IEnumerable<ScanResult> targets = context.Data as IEnumerable<ScanResult>;
                            if (targets != null)
                            {
                                ScanResult item = (from entry in targets where entry.TargetUid == uid select entry).FirstOrDefault(); //data[computerKey];
                                if (item != null)
                                    e.DataSources.Add(new ReportDataSource(context.DataSourceName, item.GetUnknownFiles()));
                            }
                        }
                    }
                    else if (e.ReportPath == "MaliciousFiles")
                    {
                        var context = (from subReport in subReports where subReport.ReportName == e.ReportPath select subReport).FirstOrDefault();
                        if (context != null)
                        {
                            IEnumerable<ScanResult> targets = context.Data as IEnumerable<ScanResult>;
                            if (targets != null)
                            {
                                ScanResult item = (from entry in targets where entry.TargetUid == uid select entry).FirstOrDefault(); //data[computerKey];
                                if (item != null)
                                    e.DataSources.Add(new ReportDataSource(context.DataSourceName, item.GetFilesByStatus(CloudVerdict.Malware)));
                            }
                        }
                    }
                };
                localReport.SubreportProcessing += subReportProcessingDelegate;
                //===========================================================================


                Stream reportContentStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(reportSourceFile);
                localReport.LoadReportDefinition(reportContentStream);

                foreach (var subReport in subReports)
                {
                    Stream subreportContentStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(subReport.FileName);
                    localReport.LoadSubreportDefinition(subReport.ReportName, subreportContentStream);
                }


                ReportParameter reportDateParam = new ReportParameter("ReportDate", DateTime.Now.ToString());
                localReport.SetParameters(new[] { reportDateParam });

                if (parameters != null && parameters.Count() > 0)
                    localReport.SetParameters(parameters);

                localReport.DataSources.Add(new ReportDataSource(dataSourceName, data));
                if (additionDataSource != null)
                {
                    foreach (var reportDataSource in additionDataSource)
                    {
                        localReport.DataSources.Add(reportDataSource);
                    }
                }


                ExportToFile(path, localReport, subReportProcessingDelegate);
            }
        }

        private static void GenerateDetailedPerProgramReportFile(string path, string dataSourceName, object data, string reportSourceFile, IEnumerable<ReportParameter> parameters, IEnumerable<ReportDataSource> additionDataSource, IEnumerable<SubReportContext> subReports)
        {
            using (LocalReport localReport = new LocalReport())
            {

                //===========================================================================
                SubreportProcessingEventHandler subReportProcessingDelegate = null;
                subReportProcessingDelegate = delegate(object sender, SubreportProcessingEventArgs e)
                {
                    string fileId = e.Parameters[0].Values[0];

                    if (e.ReportPath == "Devices")
                    {
                        var context = (from subReport in subReports where subReport.ReportName == e.ReportPath select subReport).FirstOrDefault();
                        if (context != null)
                        {
                            IEnumerable<DetailedBadFile> files = context.Data as IEnumerable<DetailedBadFile>;
                            if (files != null)
                            {
                                DetailedBadFile item = (from entry in files where entry.Hash == fileId select entry).FirstOrDefault(); //data[computerKey];
                                if (item != null)
                                    e.DataSources.Add(new ReportDataSource(context.DataSourceName, item.Targets));
                            }
                        }
                    }
                };
                localReport.SubreportProcessing += subReportProcessingDelegate;
                //===========================================================================


                Stream reportContentStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(reportSourceFile);
                localReport.LoadReportDefinition(reportContentStream);

                foreach (var subReport in subReports)
                {
                    Stream subreportContentStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(subReport.FileName);
                    localReport.LoadSubreportDefinition(subReport.ReportName, subreportContentStream);
                }


                ReportParameter reportDateParam = new ReportParameter("ReportDate", DateTime.Now.ToString());
                localReport.SetParameters(new[] { reportDateParam });

                if (parameters != null && parameters.Count() > 0)
                    localReport.SetParameters(parameters);

                localReport.DataSources.Add(new ReportDataSource(dataSourceName, data));
                if (additionDataSource != null)
                {
                    foreach (var reportDataSource in additionDataSource)
                    {
                        localReport.DataSources.Add(reportDataSource);
                    }
                }


                ExportToFile(path, localReport, subReportProcessingDelegate);
            }
        }

        private static void ExportToFile(string path, LocalReport localReport, SubreportProcessingEventHandler subReportProcessingDelegate = null)
        {
            const string exportType = "PDF";

            string mimeType;
            string encoding;
            string fileNameExtension;
            Warning[] warnings;
            string[] streams;

            byte[] renderedBytes = localReport.Render(
                exportType,
                null,
                out mimeType,
                out encoding,
                out fileNameExtension,
                out streams,
                out warnings);

            localReport.DataSources.Clear();
            localReport.SetParameters(new ReportParameter[0]);
            if (subReportProcessingDelegate != null)
                localReport.SubreportProcessing -= subReportProcessingDelegate;
            localReport.ReleaseSandboxAppDomain();

            using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write))
            {
                fs.Write(renderedBytes, 0, renderedBytes.Length);
            }
        }

        private static string GetFileName(Guid operatationUid, ReportType reportType)
        {
            const string fileFormatString = "{0}{1}.pdf";
            switch (reportType)
            {
                case ReportType.Executive:
                    return String.Format(fileFormatString, ExecutiveReportName, operatationUid);
                case ReportType.DetailedPerDevice:
                    return String.Format(fileFormatString, DetailedPerDeviceReportName, operatationUid);
                case ReportType.DetailedPerProgram:
                    return String.Format(fileFormatString, DetailedPerProgramReportName, operatationUid);
                default:
                    throw new ArgumentOutOfRangeException("reportType");
            }
        }
    }

    public class SubReportContext
    {
        public string DataSourceName { get; set; }
        public object Data { get; set; }
        public string ReportName { get; set; }
        public string FileName { get; set; }
    }
}

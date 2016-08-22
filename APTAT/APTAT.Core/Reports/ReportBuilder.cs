using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using APTAT.Core.Descent.Apt;
using Microsoft.Reporting.WinForms;
using Valkyrie.WebApiProvider.Enums;

namespace APTAT.Core.Reports
{
    public class ReportBuilder
    {
        private const string ExecutiveReportName = "ExecutiveAptReport";
        private const string DetailedPerDeviceReportName = "DetailedPerDeviceAptReport";
        private const string DetailedPerProgramReportName = "DetailedPerProgramAptReport";

        private const string ExecutiveReportNameValkyrie = "ExecutiveAptReportValkyrie";
        private const string DetailedPerDeviceReportNameValkyrie = "DetailedPerDeviceAptReportValkyrie";
        private const string DetailedPerProgramReportNameValkyrie = "DetailedPerProgramAptReportValkyrie";

        private string _fileName;

        public delegate string BuildReportDelegate(AptReportData data, ReportType reportType);
        ReportNamesCache cache = ReportNamesCache.Instance;
        public string Build(AptReportData data, ReportType reportType)
        {
            if (data == null)
                return null;                      
            
            var existentName = String.Empty;
            var flsReportKey = data.OperationUid + "-" + reportType.ToString();
            var valkyrieReportKey = data.ValkyrieOperationUid  + "-" + reportType.ToString();
            var reportKey = String.Empty;
            if (cache.Reports.Keys.Contains(flsReportKey))
            {
                existentName = cache.Reports[flsReportKey];
                reportKey = flsReportKey;
            }
            else if (cache.Reports.Keys.Contains(valkyrieReportKey))
            {
                existentName = cache.Reports[valkyrieReportKey];
                reportKey = valkyrieReportKey;
            }
            if (!String.IsNullOrWhiteSpace(existentName))
            {
                if (File.Exists(existentName))
                {
                    _fileName = existentName;
                    return existentName;
                }
                else
                {
                    cache.Reports.Remove(reportKey);
                }
            }
            
            
            _fileName = GetFileName(reportType);
            switch (reportType)
            {
                case ReportType.Executive:
                    cache.Reports.Add(data.OperationUid + "-" + reportType.ToString(), _fileName);
                    BuildExecutiveReport(_fileName, data);
                    break;
                case ReportType.ExecutiveWithValkyrie:
                    cache.Reports.Add(data.ValkyrieOperationUid + "-" + reportType.ToString(), _fileName);
                    BuildExecutiveWithValkyrieReport(_fileName, data);
                    break;
                case ReportType.DetailedPerDevice:
                    cache.Reports.Add(data.OperationUid + "-" + reportType.ToString(), _fileName);
                    BuildDetailedPerDeviceReport(_fileName, data);
                    break;
                case ReportType.DetailedPerDeviceWithValkyrie:
                    cache.Reports.Add(data.ValkyrieOperationUid + "-" + reportType.ToString(), _fileName);
                    BuildDetailedPerDeviceWithValkyrieReport(_fileName, data);
                    break;
                case ReportType.DetailedPerProgram:
                    cache.Reports.Add(data.OperationUid + "-" + reportType.ToString(), _fileName);
                    BuildDetailedPerProgramReport(_fileName, data, "APTAT.Core.Reports.DetailedPrograms.rdlc");
                    break;
                case ReportType.DetailedPerProgramWithValkyrie:
                    cache.Reports.Add(data.ValkyrieOperationUid + "-" + reportType.ToString(), _fileName);
                    BuildDetailedPerProgramReport(_fileName, data, "APTAT.Core.Reports.DetailedProgramsWithValkyrie.rdlc");
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

        private static void BuildExecutiveWithValkyrieReport(string fileName, AptReportData data)
        {
            data.GetAllBadFilesSorted(false);
            
            ReportDataSource fileRatingDataSource = new ReportDataSource("FileRatingDataSet", data.GetFileRatingCollectionValkyrie());
            ReportDataSource devicesRatingDataSource = new ReportDataSource("DevicesRatingDataSet", data.GetDevicesRatingCollectionValkyrie());

            GenerateExecutiveReportFile(fileName, "AptReportDataSet", new AptReportData[] { data }, "APTAT.Core.Reports.ExecutiveWithValkyrie.rdlc", null, fileRatingDataSource, devicesRatingDataSource);
        }

        private static void BuildDetailedPerDeviceReport(string fileName, AptReportData data)
        {
            ReportDataSource allDevicesDataSource = new ReportDataSource("ScanResultDataSet", data.SucceededTargets);
            ReportDataSource vulnerableDevicesExDataSource = new ReportDataSource("VulnerableDeviceExDataSet", data.GetTop10VulnerableDevices());
            SubReportContext unknownFilesSubReportContext = new SubReportContext() { Data = data.SucceededTargets, DataSourceName = "ModuleDataSet", FileName = "APTAT.Core.Reports.UnknownFiles.rdlc", ReportName = "UnknownFiles" };
            SubReportContext maliciousFilesSubReportContext = new SubReportContext() { Data = data.SucceededTargets, DataSourceName = "ModuleDataSet", FileName = "APTAT.Core.Reports.MaliciousFiles.rdlc", ReportName = "MaliciousFiles" };
            SubReportContext failedToAnalyzeFilesSubReportContext = new SubReportContext() { Data = data.SucceededTargets, DataSourceName = "ModuleDataSet", FileName = "APTAT.Core.Reports.FailedToAnalyzeFiles.rdlc", ReportName = "FailedToAnalyzeFiles" };
            //SubReportContext pendingAnalyzeFilesSubReportContext = new SubReportContext() { Data = data.SucceededTargets, DataSourceName = "ModuleDataSet", FileName = "APTAT.Core.Reports.PendingAnalysisFiles.rdlc", ReportName = "PendingAnalysisFiles" };
            SubReportContext noVerdictFilesSubReportContext = new SubReportContext() { Data = data.SucceededTargets, DataSourceName = "ModuleDataSet", FileName = "APTAT.Core.Reports.NoVerdictFiles.rdlc", ReportName = "NoVerdictFiles" };

            GenerateDetailedPerDevicesReportFile(fileName, "AptReportDataSet", new AptReportData[] { data }, "APTAT.Core.Reports.DetailedDevices.rdlc", null, new ReportDataSource[] { allDevicesDataSource, vulnerableDevicesExDataSource }, new SubReportContext[] { unknownFilesSubReportContext, maliciousFilesSubReportContext, failedToAnalyzeFilesSubReportContext, noVerdictFilesSubReportContext });
        }

        private static void BuildDetailedPerDeviceWithValkyrieReport(string fileName, AptReportData data)
        {
            var allBadFilesData = data.GetAllBadFilesSorted(false);
            ReportDataSource allDevicesDataSource = new ReportDataSource("ScanResultDataSet", data.SucceededTargetsWithValkyrie);
            ReportDataSource vulnerableDevicesExDataSource = new ReportDataSource("VulnerableDeviceExDataSet", data.GetTop10VulnerableDevicesValkyrie());
            //ReportDataSource valkyrieVerdictDataSource = new ReportDataSource("ValkyrieDataSet", allBadFilesData);
            SubReportContext unknownFilesSubReportContext = new SubReportContext() { Data = allBadFilesData, DataSourceName = "ValkyrieDataSet", FileName = "APTAT.Core.Reports.UnknownFilesWithValkyrie.rdlc", ReportName = "UnknownFiles" };
            SubReportContext maliciousFilesSubReportContext = new SubReportContext() { Data = allBadFilesData, DataSourceName = "ValkyrieDataSet", FileName = "APTAT.Core.Reports.MaliciousFilesWithValkyrie.rdlc", ReportName = "MaliciousFiles" };
            SubReportContext failedToAnalyzeFilesSubReportContext = new SubReportContext() { Data = allBadFilesData, DataSourceName = "ValkyrieDataSet", FileName = "APTAT.Core.Reports.FailedToAnalyzeWithValkyrie.rdlc", ReportName = "FailedToAnalyzeFiles" };
            SubReportContext pendingToAnalyzeFilesSubReportContext = new SubReportContext() { Data = allBadFilesData, DataSourceName = "ValkyrieDataSet", FileName = "APTAT.Core.Reports.PendingAnalysisFiles.rdlc", ReportName = "PendingAnalysisFiles" };                        
            //SubReportContext valkyrieVerdictSubReportContext = new SubReportContext() { Data = allBadFilesData, DataSourceName = "ValkyrieDataSet", FileName = "APTAT.Core.Reports.ValkyrieResult.rdlc", ReportName = "ValkyrieVerdict" };

            GenerateDetailedPerDevicesWithValkyrieReportFile(fileName, "AptReportDataSet", new AptReportData[] { data }, "APTAT.Core.Reports.DetailedDevicesWithValkyrie.rdlc", null, new ReportDataSource[] { allDevicesDataSource, vulnerableDevicesExDataSource/*, valkyrieVerdictDataSource*/ }, new SubReportContext[] { unknownFilesSubReportContext, maliciousFilesSubReportContext, failedToAnalyzeFilesSubReportContext, pendingToAnalyzeFilesSubReportContext /*, valkyrieVerdictSubReportContext*/ });
        }

        private static void BuildDetailedPerProgramReport(string fileName, AptReportData data, string reportName)
        {
            var allBadFilesData = data.GetAllBadFilesSorted();
            ReportDataSource allBadFilesDataSource = new ReportDataSource("DetailedBadFileDataSet", allBadFilesData);
            ReportDataSource mostWantedProgramsExDataSource = new ReportDataSource("MostWantedBadFileDataSet", data.GetTop10PopularBadFiles());
            SubReportContext devicesSubReportDataSource = new SubReportContext() { Data = allBadFilesData, DataSourceName = "ScannedTargetDataSet", FileName = "APTAT.Core.Reports.Devices.rdlc", ReportName = "Devices" };

            GenerateDetailedPerProgramReportFile(fileName, "AptReportDataSet",
                new AptReportData[] { data }, reportName,
                null,
                new ReportDataSource[] { allBadFilesDataSource, mostWantedProgramsExDataSource },
                new SubReportContext[] { devicesSubReportDataSource });
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


                ExportToFile(path, localReport);
            }
        }

        private static void GenerateDetailedPerDevicesReportFile(string path, string dataSourceName, object data, string reportSourceFile, IEnumerable<ReportParameter> parameters,
            IEnumerable<ReportDataSource> additionDataSource, IEnumerable<SubReportContext> subReports)
        {
            using (LocalReport localReport = new LocalReport())
            {

                //===========================================================================
                SubreportProcessingEventHandler subReportProcessingDelegate = null;
                subReportProcessingDelegate = delegate(object sender, SubreportProcessingEventArgs e)
                {
                    string computerKey = e.Parameters[0].Values[0];
                    Guid uid = new Guid(computerKey);


                    var context = (from subReport in subReports where subReport.ReportName == e.ReportPath select subReport).FirstOrDefault();
                    if (context != null)
                    {
                        IEnumerable<AptScanResult> targets = context.Data as IEnumerable<AptScanResult>;
                        if (targets != null)
                        {
                            AptScanResult item = (from entry in targets where entry.TargetUid == uid select entry).FirstOrDefault(); //data[computerKey];
                            if (item != null)
                            {
                                APTAT.Core.Descent.Apt.Module[] items = new APTAT.Core.Descent.Apt.Module[]{};
                                if (e.ReportPath == "UnknownFiles")
                                {
                                    items = item.GetUnknownFiles();
                                }
                                else if (e.ReportPath == "MaliciousFiles")
                                {
                                    items = item.GetFilesByStatus(CloudVerdict.Malware);                                    
                                }
                                else if (e.ReportPath == "FailedToAnalyzeFiles")
                                {
                                    items = item.GetFailedToAnalyzeFiles();
                                }
                                else if (e.ReportPath == "NoVerdictFiles")
                                {
                                    items = item.GetNoVerdictFiles();
                                }
                                else if (e.ReportPath == "PendingAnalysisFiles")
                                {                                    
                                    items = item.GetPendingAnalysisFiles();
                                }
                                
                                e.DataSources.Add(new ReportDataSource(context.DataSourceName, items));
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


                //ReportParameter reportDateParam = new ReportParameter("ReportDate", DateTime.Now.ToString());
                //localReport.SetParameters(new[] { reportDateParam });

                ReportParameter reportDateParam = new ReportParameter("ReportDate", DateTime.Now.ToString());
                ReportParameter reportDirectotyParam = new ReportParameter("IsActiveDirectorySource", ((AptReportData[])data)[0].IsActiveDirectoryDevicesSource.ToString());
                localReport.SetParameters(new[] { reportDateParam, reportDirectotyParam });

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

        private static void GenerateDetailedPerDevicesWithValkyrieReportFile(string path, string dataSourceName, object data, string reportSourceFile, IEnumerable<ReportParameter> parameters, IEnumerable<ReportDataSource> additionDataSource, IEnumerable<SubReportContext> subReports)
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
                            IEnumerable<DetailedBadFile> targets = context.Data as IEnumerable<DetailedBadFile>;
                            if (targets != null)
                            {
                                DetailedBadFile[] items = (from entry in targets where entry.Targets.Any(t => t.TargetUid == uid) select entry).ToArray();
                                if (targets != null)
                                    e.DataSources.Add(new ReportDataSource(context.DataSourceName, items.Where(f => f.FileType == BadFileType.Unknown)));
                            }
                        }
                    }
                    else if (e.ReportPath == "MaliciousFiles")
                    {
                        var context = (from subReport in subReports where subReport.ReportName == e.ReportPath select subReport).FirstOrDefault();
                        if (context != null)
                        {
                            IEnumerable<DetailedBadFile> targets = context.Data as IEnumerable<DetailedBadFile>;
                            if (targets != null)
                            {
                                DetailedBadFile[] items = (from entry in targets where entry.Targets.Any(t => t.TargetUid == uid) select entry).ToArray();
                                if (targets != null)
                                    e.DataSources.Add(new ReportDataSource(context.DataSourceName, items.Where(f => f.FileType == BadFileType.Infected)));
                            }
                        }
                    }
                    else if (e.ReportPath == "FailedToAnalyzeFiles")
                    {
                        var context = (from subReport in subReports where subReport.ReportName == e.ReportPath select subReport).FirstOrDefault();
                        if (context != null)
                        {
                            IEnumerable<DetailedBadFile> targets = context.Data as IEnumerable<DetailedBadFile>;
                            if (targets != null)
                            {
                                DetailedBadFile[] items = (from entry in targets where entry.Targets.Any(t => t.TargetUid == uid) select entry).ToArray();
                                if (targets != null)
                                    e.DataSources.Add(new ReportDataSource(context.DataSourceName, items.Where(f => f.FileType == BadFileType.NotAnalyzed)));
                            }
                        }
                    }
                    else if (e.ReportPath == "PendingAnalysisFiles")
                    {
                        var context = (from subReport in subReports where subReport.ReportName == e.ReportPath select subReport).FirstOrDefault();
                        if (context != null)
                        {
                            IEnumerable<DetailedBadFile> targets = context.Data as IEnumerable<DetailedBadFile>;
                            if (targets != null)
                            {
                                DetailedBadFile[] items = (from entry in targets where entry.Targets.Any(t => t.TargetUid == uid) select entry).ToArray();
                                if (targets != null)
                                    e.DataSources.Add(new ReportDataSource(context.DataSourceName, items.Where(f => f.FileType == BadFileType.Pending).ToList()));
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


                //ReportParameter reportDateParam = new ReportParameter("ReportDate", DateTime.Now.ToString());
                //localReport.SetParameters(new[] { reportDateParam });

                ReportParameter reportDateParam = new ReportParameter("ReportDate", DateTime.Now.ToString());
                ReportParameter reportDirectotyParam = new ReportParameter("IsActiveDirectorySource", ((AptReportData[])data)[0].IsActiveDirectoryDevicesSource.ToString());
                localReport.SetParameters(new[] { reportDateParam, reportDirectotyParam });

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
                                DetailedBadFile item = (from entry in files where entry.Sha1 == fileId select entry).FirstOrDefault(); //data[computerKey];
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
                ReportParameter reportDirectotyParam = new ReportParameter("IsActiveDirectorySource", ((AptReportData[])data)[0].IsActiveDirectoryDevicesSource.ToString());
                localReport.SetParameters(new[] { reportDateParam, reportDirectotyParam });

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

        private static string GetFileName(ReportType reportType)
        {            
            var timeStamp = DateTime.Now.ToString("s", CultureInfo.CurrentCulture).Replace(":","").Replace("-","");
            // .ToString("yyyyMMddHHmmss");
            const string fileFormatString = "{0}-{1}.pdf";
            switch (reportType)
            {
                case ReportType.Executive:
                    return String.Format(fileFormatString, ExecutiveReportName, timeStamp);
                case ReportType.ExecutiveWithValkyrie:
                    return String.Format(fileFormatString, ExecutiveReportNameValkyrie, timeStamp);
                case ReportType.DetailedPerDevice:
                    return String.Format(fileFormatString, DetailedPerDeviceReportName, timeStamp);
                case ReportType.DetailedPerDeviceWithValkyrie:
                    return String.Format(fileFormatString, DetailedPerDeviceReportNameValkyrie, timeStamp);
                case ReportType.DetailedPerProgram:
                    return String.Format(fileFormatString, DetailedPerProgramReportName, timeStamp);
                case ReportType.DetailedPerProgramWithValkyrie:
                    return String.Format(fileFormatString, DetailedPerProgramReportNameValkyrie, timeStamp);

                default:
                    throw new ArgumentOutOfRangeException("reportType");
            }
        }
    }
    
    public sealed class ReportNamesCache
    {
        private static readonly ReportNamesCache instance = new ReportNamesCache();
        static ReportNamesCache() { }
        private ReportNamesCache() { }
        public Dictionary<string, string> Reports = new Dictionary<string, string>();
        public static ReportNamesCache Instance
        {
            get { return instance; }
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

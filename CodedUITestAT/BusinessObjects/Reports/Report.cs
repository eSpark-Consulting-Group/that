using System;
using System.IO;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CodedUITestAT
{
    /// <summary> Represents a report. </summary>
    internal sealed class Report
    {
        /// <summary> A report type. </summary>
        internal ReportType Type { get { return _type; } }
        /// <summary> A directory containing the report. </summary>
        internal string ContainingDirectory { get { return _containingDirectory; } }
        /// <summary> A report file name pattern. </summary>
        internal string FileNamePattern { get { return _fileNamePattern; } }
        /// <summary> Expected report creation time. </summary>
        internal DateTime ExpectedCreationTime { get { return _expectedCreationTime; } }
        /// <summary> A report title. </summary>
        internal string Title { get { return _title; } }

        /// <summary> Represents a report. </summary>
        /// <param name="reportType">Set a report type.</param>
        internal Report(ReportType reportType) : this (reportType, DateTime.Now) { }

        /// <summary> Represents a report. </summary>
        /// <param name="reportType">Set a report type.</param>
        /// <param name="expectedCreationTime">Set an expected creation time.</param>
        internal Report(ReportType reportType, DateTime expectedCreationTime)
        {
            CodedUITestReports useReports = new CodedUITestReports();
            
            _type = reportType;

            _containingDirectory = SysHelper.GetProductDirectory();

            switch (reportType)
            {
                case ReportType.Executive:
                    _fileNamePattern = Settings.Reports.Default.ExecutiveReportFileNameBeginning;
                    _title = Settings.Reports.Default.ExecutiveReportTitle;
                    _selectReportMenuItem = useReports.UIMapReports.SelectExecutiveMenuItem;
                    break;
                case ReportType.PerDevice:
                    _fileNamePattern = Settings.Reports.Default.PerDeviceReportFileNameBeginning;
                    _title = Settings.Reports.Default.PerDeviceReportTitle;
                    _selectReportMenuItem = useReports.UIMapReports.SelectPerDeviceMenuItem;
                    break;
                case ReportType.PerProgram:
                    _fileNamePattern = Settings.Reports.Default.PerProgramReportFileNameBeginning;
                    _title = Settings.Reports.Default.PerProgramReportTitle;
                    _selectReportMenuItem = useReports.UIMapReports.SelectPerProgramMenuItem;
                    break;
                case ReportType.ExecutiveValkyrie:
                    _fileNamePattern = Settings.Reports.Default.ExecutiveValkyrieReportFileNameBeginning;
                    _title = Settings.Reports.Default.ExecutiveReportTitle;
                    _selectReportMenuItem = useReports.UIMapReports.SelectExecutiveValkyrieMenuItem;
                    break;
                case ReportType.PerDeviceValkyrie:
                    _fileNamePattern = Settings.Reports.Default.PerDeviceValkyrieReportFileNameBeginning;
                    _title = Settings.Reports.Default.PerDeviceReportTitle;
                    _selectReportMenuItem = useReports.UIMapReports.SelectPerDeviceValkyrieMenuItem;
                    break;
                case ReportType.PerProgramValkyrie:
                    _fileNamePattern = Settings.Reports.Default.PerProgramValkyrieReportFileNameBeginning;
                    _title = Settings.Reports.Default.PerProgramReportTitle;
                    _selectReportMenuItem = useReports.UIMapReports.SelectPerProgramValkyrieMenuItem;
                    break;
                default: throw new Exception(String.Format("Unexpected report type '{0}'!", reportType));
            }

            _fileNamePattern += String.Format(Settings.Reports.Default.ReportFilesEndingPattern, expectedCreationTime.ToString("yyyyMMdd")) + Settings.Reports.Default.ReportFilesExtension;

            _expectedCreationTime = expectedCreationTime;
        }

        /// <summary> Determines whether the report file exists. </summary>
        internal bool IsThereReportFile()
        {
            return IsThereReportFile(_containingDirectory, _fileNamePattern, _expectedCreationTime);
        }
        
        /// <summary> Determines whether the report file exists using its creation time as a criterion. </summary>
        /// <param name="reportDirectory">Set the report directory.</param>
        /// <param name="searchPattern">Set the report file search pattern.</param>
        /// <param name="expectedCreationTime">Set the report file expected time.</param>
        private bool IsThereReportFile(string reportDirectory, string searchPattern, DateTime expectedCreationTime)
        {
           string[] files = Directory.GetFiles(reportDirectory, searchPattern);

           if (files.Length > 0) foreach (string file in files) if (File.GetCreationTime(file).CompareTo(expectedCreationTime) >= 0) return true; 
           
           return false;
        }
        
        /// <summary> Waits for a report file creation. </summary>
        internal void WaitForReportFile()
        {
            var timer = Stopwatch.StartNew();
            for (; ; )
            {
                Playback.Wait(1000);
                if (IsThereReportFile()) break;

                if ( (int) timer.ElapsedMilliseconds > Settings.WaitingTime.Default.ReportCreationWaitTime)
                    throw new Exception(String.Format("There is no report file after {0} seconds!", Settings.WaitingTime.Default.ReportCreationWaitTime / 1000));
            }
            timer.Stop();
        }

        /// <summary> Starts the report generation via UI. Starts at Main Screen. </summary>
        internal void Generate()
        {
            _expectedCreationTime = DateTime.Now;
            CodedUITestReports useReports = new CodedUITestReports();
            useReports.UIMapReports.ClickReportsDropDownButton();
            _selectReportMenuItem();
        }

        private ReportType _type;
        private string _containingDirectory;
        private string _fileNamePattern;
        private DateTime _expectedCreationTime;
        private string _title;
        private UIAction _selectReportMenuItem;
    }
}

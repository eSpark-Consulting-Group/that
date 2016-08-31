using Xunit;
using DDT.Core.Reports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDT.Core.Reports.Tests
{
    public class ReportNamesCacheTests
    {
        [Fact()]
        public void FindTestNotExisting()
        {
            ReportNamesCache cache = new ReportNamesCache();
            var result = cache.Find(Guid.NewGuid().ToString(), ReportType.DeploymentReport, ReportFormat.PDF);
            Assert.Null(result);
        }

        [Fact()]
        public void FindAndAddTest()
        {
            string fileName = "report.pdf";
            ReportNamesCache cache = new ReportNamesCache();
            var uid = Guid.NewGuid().ToString();
            cache.Add(uid, ReportType.DeploymentReport,  ReportFormat.PDF, fileName);
            var result = cache.Find(uid, ReportType.DeploymentReport, ReportFormat.PDF);
            Assert.Equal(fileName, result);
        }


        [Fact()]
        public void AddFindDeleteTest()
        {
            string fileName = "report.pdf";
            ReportNamesCache cache = new ReportNamesCache();
            var uid = Guid.NewGuid().ToString();

            cache.Add(uid, ReportType.DeploymentReport, ReportFormat.PDF, fileName);
            var result = cache.Find(uid, ReportType.DeploymentReport,ReportFormat.PDF);
            Assert.Equal(fileName, result);

            cache.Remove(uid, ReportType.DeploymentReport, ReportFormat.PDF);
            var result2 = cache.Find(uid, ReportType.DeploymentReport, ReportFormat.PDF);
            Assert.Null(result2);
        }


        [Fact()]
        public void DeleteNotExistingTest()
        {
            
            ReportNamesCache cache = new ReportNamesCache();
            
            cache.Remove(Guid.NewGuid().ToString(), ReportType.DeploymentReport, ReportFormat.PDF);
            
            
        }
    }
}
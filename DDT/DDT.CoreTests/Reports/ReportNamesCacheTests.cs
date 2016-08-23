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
            var result = cache.Find(Guid.NewGuid().ToString(), ReportType.DeploymentReport);
            Assert.Null(result);
        }

        [Fact()]
        public void FindAndAddTest()
        {
            string fileName = "report.pdf";
            ReportNamesCache cache = new ReportNamesCache();
            var uid = Guid.NewGuid().ToString();
            cache.Add(uid, ReportType.DeploymentReport, fileName);
            var result = cache.Find(uid, ReportType.DeploymentReport);
            Assert.Equal(fileName, result);
        }


        [Fact()]
        public void AddFindDeleteTest()
        {
            string fileName = "report.pdf";
            ReportNamesCache cache = new ReportNamesCache();
            var uid = Guid.NewGuid().ToString();

            cache.Add(uid, ReportType.DeploymentReport, fileName);
            var result = cache.Find(uid, ReportType.DeploymentReport);
            Assert.Equal(fileName, result);

            cache.Remove(uid, ReportType.DeploymentReport);
            var result2 = cache.Find(uid, ReportType.DeploymentReport);
            Assert.Null(result2);
        }


        [Fact()]
        public void DeleteNotExistingTest()
        {
            
            ReportNamesCache cache = new ReportNamesCache();
            
            cache.Remove(Guid.NewGuid().ToString(), ReportType.DeploymentReport);
            
            
        }
    }
}
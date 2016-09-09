using Xunit;
using DDT.Core.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telerik.JustMock;
using DDT.Core.Reports;

namespace DDT.Core.Helper.Tests
{
    public class CSVHelperTests
    {
        private class TestConditionByStatus
        {
            public DeploymentStatus Status { get; set; }
            public int Count { get; set; }
        }
        private static List<IDetailsOfScan> GetDetailsOfScan(List<TestConditionByStatus> conditions, string ipStart, bool sameTargetUId = false)
        {

            List<IDetailsOfScan> result = new List<IDetailsOfScan>();
            Guid targetUID = Guid.NewGuid();
            for (int j = 0; j < conditions.Count; j++)
            {
                for (int i = 0; i < conditions[j].Count; i++)
                {
                    var details = Mock.Create<IDetailsOfScan>(Behavior.Loose);
                    Mock.Arrange(() => details.IP).Returns(ipStart + "." + j);
                    if (sameTargetUId)
                    {
                        Mock.Arrange(() => details.TargetUid).Returns(targetUID);
                    }
                    else
                    {
                        Mock.Arrange(() => details.TargetUid).Returns(Guid.NewGuid());
                    }
                    Mock.Arrange(() => details.DeploymentStatus).Returns(conditions[j].Status);
                    Mock.Arrange(() => details.SystemFamilyKind).Returns(APTAT.DeploymentCore.SystemFamilyKind.Windows);
                    result.Add(details);
                }
            }
            return result;


        }
        [Fact()]
        public void ToCsvTest()
        {
            
            var testConditions = new List<TestConditionByStatus>();
            testConditions.Add(new TestConditionByStatus() { Status = DeploymentStatus.Succeded, Count = 10 });
            testConditions.Add(new TestConditionByStatus() { Status = DeploymentStatus.Failed, Count = 3 });
            testConditions.Add(new TestConditionByStatus() { Status = DeploymentStatus.Skipped, Count = 5 });
            testConditions.Add(new TestConditionByStatus() { Status = DeploymentStatus.Failed, Count = 8 });
            testConditions.Add(new TestConditionByStatus() { Status = DeploymentStatus.Stopped, Count = 1 });
            List<IDetailsOfScan> d = GetDetailsOfScan(testConditions, "10.100.65");

            var result = CSVHelper.ToCsv(",",d);

            Assert.NotNull(result);
        }
    }
}
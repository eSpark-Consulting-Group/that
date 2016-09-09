using Xunit;
using DDT.Core.Reports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telerik.JustMock;
using APTAT.Common;

namespace DDT.Core.Reports.Tests
{
    public class ReportDataTests
    {
        private class TestConditionByStatus
        {
            public DeploymentStatus Status { get; set; }
            public int Count { get; set; }            
        }

        private static List<IDetailsOfScan> GetDetailsOfScan(List<TestConditionByStatus> conditions, string ipStart, bool sameTargetUId = false)
        {

            List<IDetailsOfScan> result = new List<IDetailsOfScan>();
            Guid targetUID  = Guid.NewGuid();
            for (int j = 0; j < conditions.Count; j++)
            {
                for (int i = 0; i < conditions[j].Count; i++)
                {
                    var details = Mock.Create<IDetailsOfScan>(Behavior.Strict);
                    Mock.Arrange(() => details.IP).Returns(ipStart+ "." +j);
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
        public void TestCalculations()
        {
            var testConditions = new List<TestConditionByStatus>();
            testConditions.Add(new TestConditionByStatus() { Status = DeploymentStatus.Succeded, Count = 10 });
            testConditions.Add(new TestConditionByStatus() { Status = DeploymentStatus.Failed, Count = 3 });
            testConditions.Add(new TestConditionByStatus() { Status = DeploymentStatus.Skipped, Count = 5 });
            testConditions.Add(new TestConditionByStatus() { Status = DeploymentStatus.Failed, Count = 8 });
            testConditions.Add(new TestConditionByStatus() { Status = DeploymentStatus.Stopped, Count = 1 });
            var d = GetDetailsOfScan(testConditions, "10.100.65");


            var testConditions2 = new List<TestConditionByStatus>();
            testConditions2.Add(new TestConditionByStatus() { Status = DeploymentStatus.Succeded, Count = 6 });
            testConditions2.Add(new TestConditionByStatus() { Status = DeploymentStatus.Failed, Count = 4 });
            testConditions2.Add(new TestConditionByStatus() { Status = DeploymentStatus.Skipped, Count = 7 });
            testConditions2.Add(new TestConditionByStatus() { Status = DeploymentStatus.Failed, Count = 2 });
            testConditions2.Add(new TestConditionByStatus() { Status = DeploymentStatus.Stopped, Count = 3 });
            var d2 = GetDetailsOfScan(testConditions2, "10.100.66");

            var testConditions3 = new List<TestConditionByStatus>();
            testConditions3.Add(new TestConditionByStatus() { Status = DeploymentStatus.Succeded, Count = 2 });
            testConditions3.Add(new TestConditionByStatus() { Status = DeploymentStatus.Failed, Count = 0 });
            testConditions3.Add(new TestConditionByStatus() { Status = DeploymentStatus.Skipped, Count = 0 });
            testConditions3.Add(new TestConditionByStatus() { Status = DeploymentStatus.Failed, Count = 0 });
            testConditions3.Add(new TestConditionByStatus() { Status = DeploymentStatus.Stopped, Count = 4 });
            var d3 = GetDetailsOfScan(testConditions3, "10.100.67");

            IDeploymentReportModel model1 = Mock.Create<IDeploymentReportModel>(Behavior.Strict);
            Mock.Arrange(() => model1.Details).Returns(d);

            IDeploymentReportModel model2 = Mock.Create<IDeploymentReportModel>();
            Mock.Arrange(() => model2.Details).Returns(d2);

            IDeploymentReportModel model3 = Mock.Create<IDeploymentReportModel>();
            Mock.Arrange(() => model3.Details).Returns(d3);


            ReportData data = new ReportData(model1, model2, model3);
            Assert.Equal(55, data.FoundDevices);            
            Assert.Equal(17, data.FailedDeployments);            
            Assert.Equal(8, data.StoppedDeployments);
            Assert.Equal(12, data.SkippedDeployments);
            Assert.Equal(18, data.SuccessfulDeployments);
        }


        [Fact()]
        public void TestCalculationsAfterMergeSameTargetUID()
        {
            var testConditions = new List<TestConditionByStatus>();
            testConditions.Add(new TestConditionByStatus() { Status = DeploymentStatus.Succeded, Count = 10 });
            testConditions.Add(new TestConditionByStatus() { Status = DeploymentStatus.Failed, Count = 3 });
            testConditions.Add(new TestConditionByStatus() { Status = DeploymentStatus.Skipped, Count = 5 });
            testConditions.Add(new TestConditionByStatus() { Status = DeploymentStatus.Failed, Count = 8 });
            testConditions.Add(new TestConditionByStatus() { Status = DeploymentStatus.Stopped, Count = 1 });
            var d = GetDetailsOfScan(testConditions, "10.100.65", true);

            var testConditions2 = new List<TestConditionByStatus>();
            testConditions2.Add(new TestConditionByStatus() { Status = DeploymentStatus.Succeded, Count = 10 });
            testConditions2.Add(new TestConditionByStatus() { Status = DeploymentStatus.Failed, Count = 3 });
            testConditions2.Add(new TestConditionByStatus() { Status = DeploymentStatus.Skipped, Count = 5 });
            testConditions2.Add(new TestConditionByStatus() { Status = DeploymentStatus.Failed, Count = 8 });
            testConditions2.Add(new TestConditionByStatus() { Status = DeploymentStatus.Stopped, Count = 1 });
            var d2 = GetDetailsOfScan(testConditions2, "10.100.65", true);


            var testConditions3 = new List<TestConditionByStatus>();
            testConditions3.Add(new TestConditionByStatus() { Status = DeploymentStatus.Succeded, Count = 10 });
            testConditions3.Add(new TestConditionByStatus() { Status = DeploymentStatus.Failed, Count = 3 });
            testConditions3.Add(new TestConditionByStatus() { Status = DeploymentStatus.Skipped, Count = 5 });
            testConditions3.Add(new TestConditionByStatus() { Status = DeploymentStatus.Failed, Count = 8 });
            testConditions3.Add(new TestConditionByStatus() { Status = DeploymentStatus.Stopped, Count = 1 });
            var d3 = GetDetailsOfScan(testConditions2, "10.100.65", true);


            IDeploymentReportModel model1 = Mock.Create<IDeploymentReportModel>(Behavior.Strict);
            Mock.Arrange(() => model1.Details).Returns(d);

            IDeploymentReportModel model2 = Mock.Create<IDeploymentReportModel>();
            Mock.Arrange(() => model2.Details).Returns(d2);

            IDeploymentReportModel model3 = Mock.Create<IDeploymentReportModel>();
            Mock.Arrange(() => model3.Details).Returns(d3);
                    

            ReportData data = new ReportData(model1, model2, model3);            
            Assert.Equal(3, data.FoundDevices);
            

        }


        [Fact()]
        public void TestCalculationsAfterMergeUniqueTargetUID()
        {
            var testConditions = new List<TestConditionByStatus>();
            testConditions.Add(new TestConditionByStatus() { Status = DeploymentStatus.Succeded, Count = 10 });
            testConditions.Add(new TestConditionByStatus() { Status = DeploymentStatus.Failed, Count = 3 });
            testConditions.Add(new TestConditionByStatus() { Status = DeploymentStatus.Skipped, Count = 5 });
            testConditions.Add(new TestConditionByStatus() { Status = DeploymentStatus.Failed, Count = 8 });
            testConditions.Add(new TestConditionByStatus() { Status = DeploymentStatus.Stopped, Count = 1 });
            var d = GetDetailsOfScan(testConditions, "10.100.65", false);

            var testConditions2 = new List<TestConditionByStatus>();
            testConditions2.Add(new TestConditionByStatus() { Status = DeploymentStatus.Succeded, Count = 10 });
            testConditions2.Add(new TestConditionByStatus() { Status = DeploymentStatus.Failed, Count = 3 });
            testConditions2.Add(new TestConditionByStatus() { Status = DeploymentStatus.Skipped, Count = 5 });
            testConditions2.Add(new TestConditionByStatus() { Status = DeploymentStatus.Failed, Count = 8 });
            testConditions2.Add(new TestConditionByStatus() { Status = DeploymentStatus.Stopped, Count = 1 });
            var d2 = GetDetailsOfScan(testConditions2, "10.100.65", false);


            var testConditions3 = new List<TestConditionByStatus>();
            testConditions3.Add(new TestConditionByStatus() { Status = DeploymentStatus.Succeded, Count = 10 });
            testConditions3.Add(new TestConditionByStatus() { Status = DeploymentStatus.Failed, Count = 3 });
            testConditions3.Add(new TestConditionByStatus() { Status = DeploymentStatus.Skipped, Count = 5 });
            testConditions3.Add(new TestConditionByStatus() { Status = DeploymentStatus.Failed, Count = 8 });
            testConditions3.Add(new TestConditionByStatus() { Status = DeploymentStatus.Stopped, Count = 1 });
            var d3 = GetDetailsOfScan(testConditions2, "10.100.65", false);


            IDeploymentReportModel model1 = Mock.Create<IDeploymentReportModel>(Behavior.Strict);
            Mock.Arrange(() => model1.Details).Returns(d);

            IDeploymentReportModel model2 = Mock.Create<IDeploymentReportModel>();
            Mock.Arrange(() => model2.Details).Returns(d2);

            IDeploymentReportModel model3 = Mock.Create<IDeploymentReportModel>();
            Mock.Arrange(() => model3.Details).Returns(d3);


            ReportData data = new ReportData(model1, model2, model3);
            Assert.Equal(81, data.FoundDevices);


        }

    }
}
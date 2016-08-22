//using Xunit;
//using DDT.Application.Services;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using DDT.Application.Models;
//using APTAT.DeploymentCore;
//using System.Threading;

//namespace DDT.Application.Services.Tests
//{
//    public class ReportServiceTests
//    {
//   //     [Fact()]
//        public async void BuildDiscoveryAndDeploymentReportTest()
//        {
//            ReportService reportService = new ReportService();
//            var list = new List<DeploymentItem>();
//            list.Add(new DeploymentItem() { TargetUid = Guid.NewGuid(), DnsName = "someName", IP = "10.100.65.102", IsAptHost = false, Name = "1Computer Test1", IsOnline = true, SystemKind = SystemKind.Windows81 });
//            list.Add(new DeploymentItem() { TargetUid = Guid.NewGuid(), DnsName = "someName", IP = "10.100.65.102", IsAptHost = false, Name = "1Computer Test2", IsOnline = true, SystemKind = SystemKind.Windows10 });
//            list.Add(new DeploymentItem() { TargetUid = Guid.NewGuid(), DnsName = "someName", IP = "10.100.65.102", IsAptHost = false, Name = "1Computer Test3", IsOnline = true, SystemKind = SystemKind.Windows7 });
//            list.Add(new DeploymentItem() { TargetUid = Guid.NewGuid(), DnsName = "someName", IP = "10.100.65.102", IsAptHost = false, Name = "1Computer Test4", IsOnline = true, SystemKind = SystemKind.Linux });
//            list.Add(new DeploymentItem() { TargetUid = Guid.NewGuid(), DnsName = "someName", IP = "10.100.65.102", IsAptHost = false, Name = "1Computer Test4", IsOnline = true, SystemKind = SystemKind.Linux });
//            list.Add(new DeploymentItem() { TargetUid = Guid.NewGuid(), DnsName = "someName", IP = "10.100.65.102", IsAptHost = false, Name = "1Computer Test4", IsOnline = true, SystemKind = SystemKind.Linux });
//            list.Add(new DeploymentItem() { TargetUid = Guid.NewGuid(), DnsName = "someName", IP = "10.100.65.102", IsAptHost = false, Name = "1Computer Test4", IsOnline = true, SystemKind = SystemKind.Linux });
//            list.Add(new DeploymentItem() { TargetUid = Guid.NewGuid(), DnsName = "someName", IP = "10.100.65.102", IsAptHost = false, Name = "1Computer Test4", IsOnline = true, SystemKind = SystemKind.Linux });
//            var list2 = new List<DeploymentItem>();
//            list2.Add(new DeploymentItem() { TargetUid = Guid.NewGuid(), DnsName = "someName", IP = "10.100.65.100", IsAptHost = false, Name = "2Computer Test1", IsOnline = true, SystemKind = SystemKind.Windows81 });
//            list2.Add(new DeploymentItem() { TargetUid = Guid.NewGuid(), DnsName = "someName", IP = "10.100.65.32", IsAptHost = false, Name = "2Computer Test2", IsOnline = true, SystemKind = SystemKind.Windows10 });
//            list2.Add(new DeploymentItem() { TargetUid = Guid.NewGuid(), DnsName = "someName", IP = "10.100.65.142", IsAptHost = false, Name = "2Computer Test3", IsOnline = true, SystemKind = SystemKind.Windows7 });
//            list2.Add(new DeploymentItem() { TargetUid = Guid.NewGuid(), DnsName = "someName", IP = "10.100.65.156", IsAptHost = false, Name = "2Computer Test4", IsOnline = true, SystemKind = SystemKind.Linux });

//            var list3 = new List<DeploymentItem>();
//            list3.Add(new DeploymentItem() { TargetUid = Guid.NewGuid(), DnsName = "someName", IP = "10.100.65.105", IsAptHost = false, Name = "Windows81 Test1", IsOnline = true, SystemKind = SystemKind.Windows81 });
//            list3.Add(new DeploymentItem() { TargetUid = Guid.NewGuid(), DnsName = "someName", IP = "10.100.65.106", IsAptHost = false, Name = "Windows10 Test2", IsOnline = true, SystemKind = SystemKind.Windows10 });
//            list3.Add(new DeploymentItem() { TargetUid = Guid.NewGuid(), DnsName = "someName", IP = "10.100.65.107", IsAptHost = false, Name = "Windows 7", IsOnline = true, SystemKind = SystemKind.Windows7 });
//            list3.Add(new DeploymentItem() { TargetUid = Guid.NewGuid(), DnsName = "someName", IP = "10.100.65.109", IsAptHost = false, Name = "Linux", IsOnline = true, SystemKind = SystemKind.Linux });

//            var list4 = new List<DeploymentItem>();
//            var result = await reportService.BuildDiscoveryAndDeploymentReport(list.ToArray(), list2.ToArray(), list3.ToArray(), Guid.NewGuid());
            
//        }
//    }
//}
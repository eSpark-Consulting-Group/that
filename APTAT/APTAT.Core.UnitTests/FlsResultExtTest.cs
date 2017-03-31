using System;
using System.Collections.Generic;
using System.Linq;
using APTAT.DeploymentCore.Descent;
using APTAT.DeploymentCore.Directory;
using APTAT.DeploymentCore.Messages;
using Telerik.JustMock;
using Xunit;
using APTAT.Common;
using System.Diagnostics;

//using Newtonsoft.Json;

namespace APTAT.Core.UnitTests
{
    //public class FlsResultExtTest
    //{
    //    [Fact(DisplayName = "JSON TEST")]
    //    public void TestJson()
    //    {
    //        string[] productIdentifiersValues = { "enp.c1.fat", "enp.c1.ufh" };

    //        List<Parameter> app_values_array =new List<Parameter>
    //        {
    //            new Parameter(){ Name= "op_sy" ,  Value="windows" },
    //            new Parameter(){ Name="op_ve",    Value="6.4"},
    //            new Parameter(){ Name= "op_pl" ,  Value="x86" },
    //            new Parameter(){ Name= "pr_ve",   Value="1.5.20801.85"}
    //        };

    //        string[] event_group_identifier = { "eg_da", "eg_da", "eg_da" , "eg_da" };
    //        string[] event_identifier =       { "ei_da", "ei_rp_fls","ei_rp_vlk", "ei_rp_info" };
            
    //        List<Parameter> event_values_array = new List<Parameter>()
    //        {
    //           new Parameter(){ Name= "ev_for_hashing" , Value="eh_pu" },
    //           new Parameter(){Name= "iu_tz",Value="GMT+0300"},
    //           new Parameter() {Name= "iu_ts" , Value="1463731773607" },
    //           new Parameter() { Name="sid",Value="EB7B9488-160B-40EA-8003-D37DD35EDDF2"},
    //           new Parameter() {Name= "ev_for_hashing" , Value="eh_fls_sf" },
    //           new Parameter() { Name="aid",Value="D929ECE8-3F8A-4D6C-AD9D-E5B4D66EE898"},
    //           new Parameter() { Name="sc_tp" , Value="102" },
    //           new Parameter(){Name= "pc_cc",Value="102"},
    //           new Parameter(){ Name="pc_sc" , Value="85" },
    //           new Parameter() { Name="tt_ff",Value="3654"},
    //           new Parameter(){ Name="u_fc" , Value="143" },
    //           new Parameter(){ Name="cl_fc",Value="3311"},
    //           new Parameter() { Name="mlc_fc" , Value="200" },
    //           new Parameter(){ Name="sd",Value="1479913355"},
    //           new Parameter(){ Name="fd",Value="1479914466"},
    //           new Parameter() { Name="pua_fc",Value="1"},
    //           new Parameter(){ Name="no_tf_fc" , Value="34" },
    //           new Parameter() { Name="fs",Value="1479914466"}
    //        };

    //        //GeneralInfo gi = new GeneralInfo("enp","TBD","TBD", productIdentifiersValues[0], 
    //        //    event_group_identifier[2], event_identifier[2]);

    //        ////Write test on enter give any json data on exit should get GeneralInfoObject
    //        ////Generate necessary ids for session and analyses.




    //        ////
    //        //gi.Values = new List<Parameter>();
    //        //for (int i = 0; i < 3; i++)
    //        //{
    //        //    foreach (var param in app_values_array)
    //        //    {
    //        //        gi.Values.Add(param);
    //        //    }
    //        //}

    //        ////
    //        //gi.EventsInfo = new List<EventInfo>();
    //        //for (int i = 0; i < 3; i++)
    //        //{
    //        //    EventInfo ei = new EventInfo();
    //        //    ei.EventGroupIdentifier = event_group_identifier[i];
    //        //    ei.EventIdentifier = event_identifier[i];
    //        //    ei.Values = new List<Parameter>();
    //        //    foreach (var param in event_values_array)
    //        //    {
    //        //        ei.Values.Add(param);
    //        //    }
    //        //    gi.EventsInfo.Add(ei);
    //        //}
    //        //var jsondata = JsonConvert.SerializeObject(gi);
    //        //var jsondatades = JsonConvert.DeserializeObject<GeneralInfo>(jsondata);

    //        ICMCWebApiProvider provider = new CMCWebApiProvider();
    //        var uri = new Uri(@"http://valkyrie.comodo.com");

    //        var keyProviderMock = Mock.Create<IMachineKeyProvider>();

    //        provider.SetBaseUri(uri);
    //        CMCClient client = new CMCClient(provider, keyProviderMock);
    //        client.Launched();


    //        FLSScanFinishedRequest fsfr = new FLSScanFinishedRequest()
    //        {
    //            CountOfCleanFiles = 3,
    //            CountOfMaliciousFiles = 0,
    //            CountOfScannedComputers = 32,
    //            CountOfUnknownFiles = 3,
    //            FinishedDate = DateTime.Now.AddDays(3),
    //            ScanType = ScanType.ADScan,
    //            StartedDate = DateTime.Now,
    //            TotalCountOfDiscoveredComputers = 4,
    //            TotalCountOfScannedFiles = 5
    //        };
    //        client.FLSScanFinished(fsfr);


    //        ValkyrieAnalyzeIsFinishedRequest vaifr = new ValkyrieAnalyzeIsFinishedRequest()
    //        {
    //            CountOfCleanFiles = 3,
    //            CountOfMaliciousFiles = 3,
    //            CountOfPotentiallyUnwantedApplications = 3,
    //            CountOfSendingFilesForAutamaticAnalysis = 3,
    //            CountOfUnknownFiles = 3,
    //            FinishedDate = DateTime.Now.AddDays(4),
    //            StartedDate = DateTime.Now,
    //            TotalCountOfScannedFiles = 4
    //        };
    //        client.ValkyrieAnalyzedFinished(vaifr);


    //        FLSScanUpdatedRequest fsur = new FLSScanUpdatedRequest() {
    //             CountOfCleanFiles=3,
    //             CountOfMaliciousFiles=3,
    //             CountOfUnknownFiles=2,
    //             TotalCountOfScannedFiles=33
    //        };
    //        client.FLSScanUpdated(fsur);


    //        ValkyrieAnalyzeIsUpdatedRequest vaisur = new ValkyrieAnalyzeIsUpdatedRequest() {
    //             CountOfCleanFiles=5,
    //             CountOfMaliciousFiles=3,
    //             CountOfSendingFilesForAutamaticAnalysis=3,
    //             CountOfUnknownFiles=3,
    //             FinishedDate=DateTime.Now.AddDays(4),
    //             StartedDate=DateTime.Now,
    //             TotalCountOfScannedFiles=32
    //        };
    //        client.ValkyrieAnalizelsUpdated(vaisur);


    //        ReportGeneratedRequest rgr = new ReportGeneratedRequest()
    //        {
    //            ReportId = Guid.NewGuid().ToString()
    //        };
    //        client.ReportGenerated(rgr);


    //    }









    //    [Theory(DisplayName = "Test method IsNotAnalyzed for analysed files.")]
    //    [InlineData(Descent.Apt.CloudVerdict.FailedToAnalyze)]
    //    [InlineData(Descent.Apt.CloudVerdict.NoVerdict)]
    //    public void TestExtensionMethodIsNotAnalyzedForAnalysed(Descent.Apt.CloudVerdict verdict)
    //    {
    //        //Arrange
    //        IFlsResult testdata = Mock.Create<IFlsResult>(Behavior.Strict);
    //        Mock.Arrange(() => testdata.Cloud).Returns(verdict);
    //        //Act
    //        var result = FlsResultExt.IsNotAnalyzed(testdata);
    //        //Assert
    //        Assert.True(result);
    //    }
    //    [Theory(DisplayName = "Test method IsNotAnalyzed for not analysed files.")]
    //    [InlineData(Descent.Apt.CloudVerdict.FirstSeen)]
    //    [InlineData(Descent.Apt.CloudVerdict.Malware)]
    //    [InlineData(Descent.Apt.CloudVerdict.Safe)]
    //    [InlineData(Descent.Apt.CloudVerdict.Unknown)]
    //    public void TestExtensionMethodIsNotAnalyzedForNotAnalysed(Descent.Apt.CloudVerdict verdict)
    //    {
    //        //Arrange
    //        IFlsResult testdata = Mock.Create<IFlsResult>(Behavior.Strict);
    //        Mock.Arrange(() => testdata.Cloud).Returns(verdict);
    //        //Act
    //        var result = FlsResultExt.IsNotAnalyzed(testdata);
    //        //Assert
    //        Assert.False(result);
    //    }
    //    [Theory(DisplayName = "Test method IsUnknown for unknown files.")]
    //    [InlineData(Descent.Apt.CloudVerdict.FirstSeen)]
    //    [InlineData(Descent.Apt.CloudVerdict.Unknown)]
    //    public void TestExtensionMethodIsUnknownForNotAnalysed(Descent.Apt.CloudVerdict verdict)
    //    {
    //        //Arrange
    //        IFlsResult testdata = Mock.Create<IFlsResult>(Behavior.Strict);

    //        Mock.Arrange(() => testdata.Cloud).Returns(verdict);
    //        //Act
    //        var result = FlsResultExt.IsUnknown(testdata);
    //        //Assert
    //        Assert.True(result);
    //    }
    //    [Theory(DisplayName = "Test method IsUnknown for known files.")]
    //    [InlineData(Descent.Apt.CloudVerdict.FailedToAnalyze)]
    //    [InlineData(Descent.Apt.CloudVerdict.NoVerdict)]
    //    [InlineData(Descent.Apt.CloudVerdict.Malware)]
    //    [InlineData(Descent.Apt.CloudVerdict.Safe)]
    //    public void TestExtensionMethodIsUnknownForAnalysed(Descent.Apt.CloudVerdict verdict)
    //    {
    //        //Arrange
    //        IFlsResult testdata = Mock.Create<IFlsResult>(Behavior.Strict);
    //        Mock.Arrange(() => testdata.Cloud).Returns(verdict);
    //        //Act
    //        var result = FlsResultExt.IsUnknown(testdata);
    //        //Assert
    //        Assert.False(result);
    //    }
    //}
}

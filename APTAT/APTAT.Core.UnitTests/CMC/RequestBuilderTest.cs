using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using APTAT.Core.CMC;
using Telerik.JustMock;
using Telerik.JustMock.Helpers;
using Xunit;
using Xunit.Extensions;

namespace APTAT.Core.UnitTests.CMC
{
    public class RequestBuilderTest
    {
        private static IEnumerable<object[]> BuildTestParameters(){
            var accountId = Guid.NewGuid().ToString();
            var analyzeId = Guid.NewGuid().ToString();
            var sessionId = Guid.NewGuid().ToString();
            return new[]{
                new object[] {"enp", "TBD", "TBD", "enp.c1.fat",
                    accountId,analyzeId,sessionId,"ei_da","eg_da","eh_pu",
                    true,true,true},
                new object[] {"enp", "TBD", "TBD", "enp.c1.fat",
                    accountId,analyzeId,sessionId,"ei_rp_ad","eg_da","eh_ad_adf",
                    false,true,true},
                new object[] {"enp", "TBD", "TBD", "enp.c1.fat",
                    accountId,analyzeId,sessionId,"ei_rp_sc","eg_da","eh_sc_ss",
                    true,false,false},
                new object[] {"enp", "TBD", "TBD", "enp.c1.fat",
                    accountId,analyzeId,sessionId,"ei_rp_sc","eg_da","eh_fat_sf",
                    true,false,true},
                new object[] {"enp", "TBD", "TBD", "enp.c1.fat",
                    accountId,analyzeId,sessionId,"ei_rp_sc","eg_da","eh_sc_su",
                    false,false,true},
                new object[] {"enp", "TBD", "TBD", "enp.c1.fat",
                    accountId,analyzeId,sessionId,"ei_rp_info","eg_da","eh_rp_gen",
                    false,false,false},
                new object[] {"enp", "TBD", "TBD", "enp.c1.fat",
                    accountId,analyzeId,sessionId,"ei_rp_info","eg_da","eh_rp_svd",
                    true,true,true},
                new object[] {"enp", "TBD", "TBD", "enp.c1.fat",
                    accountId,analyzeId,sessionId,"ei_da","eg_da","eh_pc",
                    true,true,true}
            };
        }

        private static List<IParameter> GetParameters(int parametersCount, bool isApplication){
            var parameters = new List<IParameter>();
            for (int i = 0; i < 5; i++){
                var parameter = Mock.Create<IParameter>();
                Mock.ArrangeSet(() => parameter.IsApplication = true);
                Mock.ArrangeSet(() => parameter.Name = "NAME_" + parametersCount);
                Mock.ArrangeSet(() => parameter.Value = "VALUE_" + parametersCount);
                parameters.Add(parameter);
            }
            return parameters;
        }


        [Fact(DisplayName = "RequestBuilder constructor test.")]
        public void RequestBuilderConstructorTest()
        {
            var generalInfoParameters = Mock.Create<IGeneralInfoParameters>();
            Mock.Arrange(() => generalInfoParameters.ApplicationIdentifier).Returns("APPID");
            Mock.Arrange(() => generalInfoParameters.GlobalMachineCode).Returns("GMCD");
            Mock.Arrange(() => generalInfoParameters.MachineCode).Returns("MCD");
            Mock.Arrange(() => generalInfoParameters.ProductIdentifier).Returns("PRID");
            RequestBuilder requestBuilder = new RequestBuilder(generalInfoParameters);

            Mock.Assert(requestBuilder);
        }


        [Theory(DisplayName = "GenerateRequest  test."), MemberData(nameof(BuildTestParameters))]
        public void GenerateRequestTest(string applicationIdentifier, string globalMachineCode,
            string machineCode, string productIdentifier, string accountIdentifier, string analizeIdentifier,
            string sessionIdentifier, string eventId, string eventGroupId, string eventParamValue,
            bool createApplicationParams, bool createEvent, bool createEventParams){

            //Arrange
            #region generalInfo mock create.
            var generalInfo = Mock.Create<IGeneralInfo>();
            if (generalInfo == null) throw new ArgumentNullException(nameof(generalInfo));
            Mock.ArrangeSet(() => generalInfo.ApplicationIdentifier = applicationIdentifier);
            Mock.ArrangeSet(() => generalInfo.GlobalMachineCode = globalMachineCode);
            Mock.ArrangeSet(() => generalInfo.MachineCode = machineCode);
            Mock.ArrangeSet(() => generalInfo.ProductIdentifier = productIdentifier);

            var applicationParameters = createApplicationParams ? GetParameters(5, true) : new List<IParameter>();

            Mock.ArrangeSet(() => generalInfo.Values = applicationParameters);

            var eventInfo = Mock.Create<IEventInfo>();
            Mock.ArrangeSet(() => eventInfo.EventGroupIdentifier = eventGroupId);
            Mock.ArrangeSet(() => eventInfo.EventIdentifier = eventId);

            var eventParameters = createEventParams ? GetParameters(5, false) : new List<IParameter>();

            Mock.ArrangeSet(() => eventInfo.EventValues = eventParameters);

            Mock.Arrange(() => eventInfo.EventValues).Returns(eventParameters);

            if (createEvent)
            {
                var eventInfoList = new List<IEventInfo>();
                eventInfoList.Add(eventInfo);
                Mock.ArrangeSet(() => generalInfo.EventsInfo = eventInfoList);
            }

            #endregion

            var generalInfoParameters = Mock.Create<IGeneralInfoParameters>();
            Mock.Arrange(() => generalInfoParameters.ApplicationIdentifier).Returns(applicationIdentifier);
            Mock.Arrange(() => generalInfoParameters.GlobalMachineCode).Returns(globalMachineCode);
            Mock.Arrange(() => generalInfoParameters.MachineCode).Returns(machineCode);
            Mock.Arrange(() => generalInfoParameters.ProductIdentifier).Returns(productIdentifier);

            RequestBuilder requestBuilder = new RequestBuilder(generalInfoParameters);

            var requstParameters = Mock.Create<IRequestParameters>();
            Mock.ArrangeSet(() => requstParameters.AccountIdentifier = accountIdentifier);
            Mock.ArrangeSet(() => requstParameters.AnalizeIdentifier = analizeIdentifier);
            Mock.ArrangeSet(() => requstParameters.SessionIdentifier = sessionIdentifier);
            Mock.ArrangeSet(() => requstParameters.EventId = eventId);
            Mock.ArrangeSet(() => requstParameters.EventGroupId = eventGroupId);
            Mock.ArrangeSet(() => requstParameters.EventParamValue = eventParamValue);

            var additionalParameters = createApplicationParams ? GetParameters(5, true) : new List<IParameter>();
            var request = Mock.Create<IRequest>(Behavior.Strict);


            //Act
            var generalInfoReal = requestBuilder.GenerateRequest(requstParameters, additionalParameters, request);

            //Assert
            Assert.Equal(generalInfoReal.ApplicationIdentifier, generalInfo.ApplicationIdentifier);
            Assert.Equal(generalInfoReal.GlobalMachineCode, generalInfo.GlobalMachineCode);
            Assert.Equal(generalInfoReal.MachineCode, generalInfo.MachineCode);
            Assert.Equal(generalInfoReal.ProductIdentifier, generalInfo.ProductIdentifier);


            var generalInfoRealValues = generalInfoReal.Values;
            var generalInfoValues = generalInfo.Values;
            for (int i = 0; i < generalInfoRealValues.Count; i++){
                Assert.Equal(generalInfoRealValues[i].Name, generalInfoValues[i].Name);
                Assert.Equal(generalInfoRealValues[i].Value, generalInfoValues[i].Value);
            }
        }

    }
}

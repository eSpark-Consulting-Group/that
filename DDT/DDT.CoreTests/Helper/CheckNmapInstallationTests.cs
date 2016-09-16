using Xunit;
using System;
using System.Linq;
using Telerik.JustMock;

namespace DDT.Core.Helper.Tests
{
    public class CheckNmapInstallationTests
    {

        [Theory()]
        [InlineData(new bool[] { true, true, true, true })]
        [InlineData(new bool[] { false, true, true, true })]
        [InlineData(new bool[] { true, false, true, true })]
        [InlineData(new bool[] { true, true, false, true })]
        [InlineData(new bool[] { true, true, true, false})]

        [InlineData(new bool[] { false, true, true, false })]
        [InlineData(new bool[] { false, false, true, true})]
 
        [InlineData(new bool[] { true, false, true, false })]
        [InlineData(new bool[] { false, true, false, true})]

        [InlineData(new bool[] { false, false, true, false })]
        [InlineData(new bool[] { false, true, false, false})]
        [InlineData(new bool[] { false, false, false, true })]
        [InlineData(new bool[] { true, false, false, false })]
        [InlineData(new bool[] { false, false, false, false })]
        public void CheckTest(bool[] conditions)
        {
            
            var procHelper = Mock.Create<IProcessHelper>();
            var regHelper = Mock.Create<IRegistryHelper>();
            var fileHelper = Mock.Create<IFileHelper>();
            var environmentHelper = Mock.Create<IEnvironmentHelper>();

            Mock.Arrange(() => procHelper.IsProcessExist(null)).IgnoreArguments().Returns(conditions[0]);
            Mock.Arrange(() => environmentHelper.Is64BitOperatingSystem()).IgnoreArguments().Returns(true);
            Mock.Arrange(() => regHelper.IsSubKeyExists(null)).IgnoreArguments().Returns(conditions[1]);
            Mock.Arrange(() => regHelper.IsValueExists(null, null)).IgnoreArguments().Returns(conditions[2]);
            Mock.Arrange(() => fileHelper.IsFileExists(null)).IgnoreArguments().Returns(conditions[3]);

            CheckNmapInstallation nmapHelper = new CheckNmapInstallation(procHelper, regHelper, fileHelper, environmentHelper);
            string result = String.Empty;
            var isReady = nmapHelper.Check(out result);
            Assert.NotNull(result);
            if (conditions.Any(c => c == false))
            {
                Assert.False(isReady);
            }
            else
            {
                Assert.True(isReady);
            }

        }


        [Theory()]
        [InlineData(new bool[] { true, true, true, true })]
        [InlineData(new bool[] { false, true, true, true })]
        [InlineData(new bool[] { true, false, true, true })]
        [InlineData(new bool[] { true, true, false, true })]
        [InlineData(new bool[] { true, true, true, false })]

        [InlineData(new bool[] { false, true, true, false })]
        [InlineData(new bool[] { false, false, true, true })]

        [InlineData(new bool[] { true, false, true, false })]
        [InlineData(new bool[] { false, true, false, true })]

        [InlineData(new bool[] { false, false, true, false })]
        [InlineData(new bool[] { false, true, false, false })]
        [InlineData(new bool[] { false, false, false, true })]
        [InlineData(new bool[] { true, false, false, false })]
        [InlineData(new bool[] { false, false, false, false })]
        public void CheckTest86(bool[] conditions)
        {

            var procHelper = Mock.Create<IProcessHelper>();
            var regHelper = Mock.Create<IRegistryHelper>();
            var fileHelper = Mock.Create<IFileHelper>();
            var environmentHelper = Mock.Create<IEnvironmentHelper>();

            Mock.Arrange(() => procHelper.IsProcessExist(null)).IgnoreArguments().Returns(conditions[0]);
            Mock.Arrange(() => environmentHelper.Is64BitOperatingSystem()).IgnoreArguments().Returns(false);
            Mock.Arrange(() => regHelper.IsSubKeyExists(null)).IgnoreArguments().Returns(conditions[1]);
            Mock.Arrange(() => regHelper.IsValueExists(null, null)).IgnoreArguments().Returns(conditions[2]);
            Mock.Arrange(() => fileHelper.IsFileExists(null)).IgnoreArguments().Returns(conditions[3]);

            CheckNmapInstallation nmapHelper = new CheckNmapInstallation(procHelper, regHelper, fileHelper, environmentHelper);
            string result = String.Empty;
            var isReady = nmapHelper.Check(out result);
            Assert.NotNull(result);
            if (conditions.Any(c => c == false))
            {
                Assert.False(isReady);
            }
            else
            {
                Assert.True(isReady);
            }

        }
    }
}
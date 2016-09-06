using Xunit;
using DDT.Core.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDT.Core.Helper.Tests
{
    public class CheckNmapInstallationTests
    {
        [Fact()]
        public void CheckTest()
        {
            CheckNmapInstallation nmapHelper = new CheckNmapInstallation();
            string result = String.Empty;
            var isReady = nmapHelper.Check(out result);
            Assert.NotNull(result);
            

        }
    }
}
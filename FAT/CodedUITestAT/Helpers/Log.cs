using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodedUITestFAT.Helpers
{
    internal class Log
    {
        private Log() { }

        private TestContext testContext;
        internal Log(TestContext testContext) {
            this.testContext = testContext;
        }

        internal void Success(string message)
        {
            //testContext.WriteLine($"SUCCESS [{message}]");
            testContext.WriteLine(String.Format("SUCCESS [{0}]", message));
        }

        internal void Failed(string message)
        {
            //testContext.WriteLine($"FAILED [{message}]");
            testContext.WriteLine(String.Format("FAILED [{0}]", message));
        }

        internal void Warning(string message)
        {
            //testContext.WriteLine($"WARNING [{message}]");
            testContext.WriteLine(String.Format("WARNING [{0}]", message));
        }
    }
}

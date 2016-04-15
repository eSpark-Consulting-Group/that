using System;
using Microsoft.VisualStudio.TestTools.UITesting;

namespace CodedUITestAT
{
    /// <summary> Contains the project global constants and gets the local environment variables stored during a test build deployment process using a batch file (find in the project). </summary>
    internal static class DataHelper
    {
        #region Constants
        
        /// <summary> Default waiting time for tests. </summary>
        internal const int DefaultWaitTime = 15000; // Milliseconds

        #endregion

        #region Environment variables

        internal static readonly string Username = Environment.GetEnvironmentVariable("_ServerLogin", EnvironmentVariableTarget.Machine);
        internal static readonly string Password = Environment.GetEnvironmentVariable("_ServerPassword", EnvironmentVariableTarget.Machine);
        //internal static readonly string Username = "administrator";
        //internal static readonly string Password = "test";
        
        internal static readonly string EncriptedPassword = Playback.EncryptText(Password);

        internal static readonly string ProductExeName = Environment.GetEnvironmentVariable("_ProductExeName", EnvironmentVariableTarget.Machine);

        /// <summary> Gets an internal name of endpoint obtained from a local environment variable. </summary>
        internal static string GetEndpointName()
        {
           return Environment.GetEnvironmentVariable("_EndpointName", EnvironmentVariableTarget.Machine).ToUpper();
           //return "as06w8x64".ToUpper();
        }

        /// <summary> Gets working folder from a local environment variable </summary>
        internal static string GetWorkingFolder()
        {
            return Environment.GetEnvironmentVariable("_TargetDir", EnvironmentVariableTarget.Machine);
            //return @"C:\AT";
        }

        #endregion
    }
}

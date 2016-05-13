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

        /// <summary> Gets a root directory of endpoint machine in the environment from a local environment variable. </summary>
        /// <param name="IsAdminShare">Determines whether the root should be returned in a form of admin share. </param>        
        internal static string GetEndpointRoot(bool isAdminShare = false)
        {
            string rootDirectory = Environment.GetEnvironmentVariable("_EndpointRoot", EnvironmentVariableTarget.Machine);
            if (isAdminShare) return rootDirectory + @"$";
            else return rootDirectory + @":\";
            /*
            if (isAdminShare) return @"C$";
            else return @"C:\";*/
        }

        /// <summary> Gets the path to endpoint folder infected during the deployment process. </summary>
        internal static string GetInfectedFolder()
        {
            return Environment.GetEnvironmentVariable("_InfectedFolder", EnvironmentVariableTarget.Machine);
            //return @"Users\Administrator\AppData\Roaming\Microsoft\Windows\Start Menu\Programs\Startup";
        }

        /// <summary> Gets a malware name copied to an endpoint during the deployment process. </summary>
        internal static string GetMalwareName()
        {
            return Environment.GetEnvironmentVariable("_MalwareName", EnvironmentVariableTarget.Machine);
            //return "91exestealth.exe";
        }

        #endregion
    }
}

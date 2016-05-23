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
        
        internal static readonly string EncriptedPassword = Playback.EncryptText(Password);

        internal static readonly string ProductExeName = Environment.GetEnvironmentVariable("_ProductExeName", EnvironmentVariableTarget.Machine);

        /// <summary> Gets data on an internal endpoint obtained from a local environment variable. </summary>
        internal static Computer GetEndpoint()
        {
           string computerName = Environment.GetEnvironmentVariable("_EndpointName", EnvironmentVariableTarget.Machine).ToUpper();
           return new Computer(computerName);
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

        /// <summary> Gets a root directory of server machine in the environment from a local environment variable. </summary>
        /// <param name="IsAdminShare">Determines whether the root should be returned in a form of admin share.</param>        
        internal static string GetServerRoot(bool isAdminShare = false)
        {
            string rootDirectory = Environment.GetEnvironmentVariable("_ServerRoot", EnvironmentVariableTarget.Machine);
            if (isAdminShare) return rootDirectory + @"$";
            else return rootDirectory + @":\";
            //return @"C:\";
        }

        /// <summary> Gets a path to a folder with malwares on server or endpoint from a local environment variable. </summary>
        /// <param name="isServer">Indicate whether the path to a folder with malwares should be formed for the product server.</param>
        internal static string GetMalwaresFolder(bool isServer = false)
        {
            string malFolder = Environment.GetEnvironmentVariable("_MalwaresFolder", EnvironmentVariableTarget.Machine);
            if (isServer) return String.Format(@"{0}{1}", GetServerRoot(), malFolder);
            else return String.Format(@"{0}{1}", GetEndpointRoot(), malFolder);

            //return @"C:\Malfolder";
        }

        #endregion
    }
}

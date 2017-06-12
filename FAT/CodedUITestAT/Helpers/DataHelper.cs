using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UITesting;

namespace CodedUITestFAT
{
    /// <summary> Contains the project global constants, variables, and gets the local environment variables stored during a test build deployment process using a batch file (find in the project). </summary>
    internal static class DataHelper
    {
        #region Global constants

        /// <summary> Default waiting time for tests. </summary>
        internal const int DefaultWaitTime = 15000; // Milliseconds

        /// <summary> Timeout for long-lasting tests. </summary>
        internal const int LongTestTimeout = 3600000; // Milliseconds

        /// <summary> Timeout for very long-lasting tests. </summary>
        internal const int LongestTestTimeout = 14400000; // Milliseconds

        #endregion

        #region Global variables

        /// <summary> Indicates whether the product is redirected to FLS staging server. </summary>
        internal static bool IsRedirectedToFlsStaging = false;

        /// <summary> Indicates whether the product is redirected to Valkyrie and CAM staging servers. </summary>
        internal static bool IsRedirectedToValkyrieStaging = false;

        /// <summary> Indicates whether the product archive is extracted. </summary>
        internal static bool IsProductExtracted = false;

        #endregion

        #region Environment variables

        /// <summary> A user name for authentication. </summary>
        internal static readonly string Username;

        /// <summary> A user password for authentication. </summary>
        internal static readonly string Password;

        internal static readonly string EncryptedPassword;

        /// <summary> The product executable name. </summary>
        internal static readonly string ProductExeName;

        /// <summary> The product process name. </summary>
        internal static readonly string ProductProcessName;

        /// <summary> Working folder. </summary>
        internal static readonly string WorkingFolder;

        /// <summary> A path to the extracted product archive. </summary>                
        internal static readonly string ExtractedProductPath;

        static DataHelper()
        {
            Username = Environment.GetEnvironmentVariable("_ServerLogin", EnvironmentVariableTarget.Machine);
            Password = Environment.GetEnvironmentVariable("_ServerPassword", EnvironmentVariableTarget.Machine);
            EncryptedPassword = Playback.EncryptText(Password);
            ProductExeName = Environment.GetEnvironmentVariable("_ProductExeName", EnvironmentVariableTarget.Machine);
            WorkingFolder = Environment.GetEnvironmentVariable("_TargetDir", EnvironmentVariableTarget.Machine);
            ExtractedProductPath = Path.Combine(WorkingFolder, ProductExeName.WithoutExtension());
            ProductProcessName = ProductExeName.WithoutExtension();
        }
        /// <summary> Determines whether the autotests will run using FLS staging server. Gets the data from a local environment variable. </summary>
        internal static bool IsFlsStaging()
        {
            string isStagingStr = Environment.GetEnvironmentVariable("_IsFlsStaging", EnvironmentVariableTarget.Machine);
            int isStaging = Int32.Parse(isStagingStr);

            if (isStaging == 0) return false;
            else return true;
        }

        /// <summary> Determines whether the autotests will run using Valkyrie staging server. Gets the data from a local environment variable. </summary>
        internal static bool IsValkyrieStaging()
        {
            string isStagingStr = Environment.GetEnvironmentVariable("_IsValkyrieStaging", EnvironmentVariableTarget.Machine);
            int isStaging = Int32.Parse(isStagingStr);

            if (isStaging == 0) return false;
            else return true;
        }

        /// <summary> Gets data on an internal endpoint obtained from a local environment variable. </summary>
        internal static Computer GetEndpoint()
        {
            string computerName = Environment.GetEnvironmentVariable("_EndpointName", EnvironmentVariableTarget.Machine);
            return new Computer(computerName);
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

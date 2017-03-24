using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UITesting;

namespace CodedUITestFAT
{
    /// <summary> Contains methods for manual testing support. To be converted to a separate solution. </summary>
    internal static class Support
    {
        /// <summary> Generates a given number of unknown files with a list of their SHA1. </summary>
        /// <param name="numberOfFiles">Set a number of files to generate.</param>
        /// <returns>A path to the generated files.</returns>
        /// <remarks>Requires a PE file and some environment varibales to be set.</remarks>
        internal static string GenerateUnknowns(int numberOfFiles)
        {
            Computer endpoint = new Computer();
            string path = Path.Combine(Settings.Scanner.Default.StartupFolder);
            List<UnknownFile> unknowns = new List<UnknownFile>();

            for (int i = 0; i < numberOfFiles; i++)
            {
                UnknownFile unknown = SysHelper.AddUnknownFileForScan(endpoint, ScanType.Quick);
                unknowns.Add(unknown);
                Playback.Wait(1000);
            }

            string hashesList = Path.Combine(Path.GetDirectoryName(unknowns[0].Location), @"SHA1_List.txt");
            var hashesListStream = File.Create(hashesList);
            hashesListStream.Close();

            using (var stream = new StreamWriter(hashesList, true, Encoding.Default))
            {
                foreach (var file in unknowns)
                {
                    string record = String.Format(@"{0} - {1}", file.Name, file.Sha1);
                    stream.WriteLine(record);
                }

                stream.WriteLine();

                foreach (var file in unknowns)
                {
                    stream.WriteLine(file.Sha1);
                }

                stream.Close();
            }

            return Path.GetDirectoryName(unknowns[0].Location);
        }

    }
}

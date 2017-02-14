using System.IO;

namespace APTAT.Core.TelerikReports
{
    public class FileHelper : IFileHelper
    {
        public bool IsFileExists(string filePath)
        {
            return File.Exists(filePath);
        }
    }
}
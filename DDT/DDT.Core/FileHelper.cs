using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDT.Core
{
    public interface IFileHelper
    {
        bool IsFileExists(string filePath);
    }
    public class FileHelper : IFileHelper
    {
        public bool IsFileExists(string filePath)
        {
            return File.Exists(filePath);
        }
    }
}

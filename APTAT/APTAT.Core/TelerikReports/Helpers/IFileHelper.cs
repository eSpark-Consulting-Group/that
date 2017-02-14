using APTAT.Core.TelerikReports;
using System;
using System.Globalization;

namespace APTAT.Core.TelerikReports
{
    public interface IFileHelper
    {
        bool IsFileExists(string filePath);
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace APTAT.Application
{
    public static class Logger
    {
        public static void Clear()
        {
            File.Delete("APTAT.Application.Log.txt");
        }

        public static void WriteLog(String text)
        {
            File.AppendAllText("APTAT.Application.Log.txt",  text + Environment.NewLine);
        }
    }
}

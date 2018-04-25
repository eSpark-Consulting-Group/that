using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UFH.Application.Extensions
{
    public static class ExceptionExtension
    {
        public static string GetFullMessage(this Exception ex)
        {
            string res = string.Empty;
            GetFullMessage(ex, ref res);
            return res;
        }

        public static string GetFullMessage(this object ex)
        {
            string res = string.Empty;
            GetFullMessage(ex as Exception, ref res);
            return res;
        }

        private static void GetFullMessage(Exception ex, ref string res, int offsetCount = 0)
        {
            if (ex != null)
            {
                res += offsetCount != 0 ? Environment.NewLine + "\t  InnerException :".Mult(offsetCount) : string.Empty
                    + ex.Message;
                GetFullMessage(ex.InnerException, ref res, ++offsetCount);
            }            
        }

        private static string Mult(this string str, int count)
        {
            string res = string.Empty;
            for (int i = 0; i < count; i++)
            {
                res += str;
            }
            return res;
        }
    }
}

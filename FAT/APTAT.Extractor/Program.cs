using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APTAT.Extractor
{
    class Program
    {
        static void Main(string[] args)
        {            
            Extractor.ExtractId(args.FirstOrDefault());
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDT.Core.Helper
{
   public class EnvironmentHelper : IEnvironmentHelper
    {
        public bool Is64BitOperatingSystem()
        {            
            return Environment.Is64BitOperatingSystem;
        }
    }
}

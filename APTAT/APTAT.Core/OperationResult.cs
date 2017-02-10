using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APTAT.Core
{
    public class OperationResult
    {
        public Exception Error { get; set; }
        public bool IsSucceeded { get; set; }

        public string ErrorMessage { get; set; }
    }
}

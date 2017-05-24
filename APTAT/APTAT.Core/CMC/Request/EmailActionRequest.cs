using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APTAT.Core.CMC.Request
{
    public class EmailActionRequest : IEmailActionRequest
    {
        public EmailAction EmailAction { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APTAT.Valkyrie.Client.Contracts
{
    public class BasicInfoRequest : RequestBase
    {
        public string SHA1 { get; set;}
    }
}

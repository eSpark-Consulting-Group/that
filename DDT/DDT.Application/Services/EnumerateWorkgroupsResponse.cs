using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDT.Application.Services
{
    public class EnumerateWorkgroupsResponse : DomainEventArgs
    {
        public EnumerateWorkgroupsResponse(string[] workgroups, Exception exception = null)
        {
            Workgroups = workgroups;
            Exception = exception;
        }

        public string[] Workgroups { get;  }
    }
}

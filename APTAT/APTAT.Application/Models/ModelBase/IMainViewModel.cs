using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APTAT.Application.Models.ModelBase
{
    public interface IMainViewModel
    {
        string Login { get;  }
        string Password { get;  }
        bool HasCurrentDomainName { get;  }
        string TcpPort { get;  }
        string UdpPort { get;  }

        bool ValidateCredentials(out string errorMessage);
    }
}

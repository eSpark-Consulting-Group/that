

namespace APTAT.Application.Models.ModelBase
{
    public interface IMainViewModel
    {
        string Login { get;  }
        string Password { get;  }
        bool HasCurrentDomainName { get; }
        bool UseCurrentCredentials { get; set; }
        string TcpPort { get;  }
        string UdpPort { get;  }

        bool ValidateCredentials(out string errorMessage);
        bool ValidatePorts(out string errorMessage);
    }
}

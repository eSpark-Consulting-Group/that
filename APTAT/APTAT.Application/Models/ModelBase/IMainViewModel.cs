

namespace APTAT.Application.Models.ModelBase
{
    public interface IMainViewModel
    {
        string Login { get; set; }
        string Password { get; set; }
        bool HasCurrentDomainName { get; }
        bool UseCurrentCredentials { get; set; }
        string TcpPort { get;  }
        string UdpPort { get;  }

        bool ValidateCredentials(out string errorMessage);
        bool ValidatePorts(out string errorMessage);

        void RestorePorts();
    }
}

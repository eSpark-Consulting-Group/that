using System;
using APTAT.Core.Descent.Apt;

namespace APTAT.Core.Apt
{
    public class AptVerdictContext : IAptVerdictContext
    {
        private CloudVerdict _cloud;
        public string Sha1 { get; }

        public CloudVerdict Cloud
        {
            get { return _cloud; }
            set
            {
                if (_cloud != value)
                {
                    _cloud = value;
                    ModifiedDateTime = DateTime.UtcNow;
                }
            }
        }
        public DateTime ModifiedDateTime { get; private set; }
        public AptVerdictContext(string sha1, CloudVerdict flsVerdict)
        {
            Sha1 = sha1;
            Cloud = flsVerdict;
            ModifiedDateTime = DateTime.UtcNow;
        }
        private AptVerdictContext(string sha1, CloudVerdict flsVerdict, DateTime modified)
            : this(sha1, flsVerdict)
        {
            ModifiedDateTime = modified;
        }
        public IAptVerdictContext Clone()
        {
            return new AptVerdictContext(Sha1, Cloud, ModifiedDateTime);
        }

        public override int GetHashCode()
        {
            return StringComparer.OrdinalIgnoreCase.GetHashCode(Sha1);
        }

        public override bool Equals(object obj)
        {
            AptVerdictContext otherFileInfo = obj as AptVerdictContext;
            return otherFileInfo != null && otherFileInfo.Sha1.Equals(Sha1, StringComparison.OrdinalIgnoreCase);
        }
    }

    public interface IAptVerdictContext : IFlsResult
    {
        DateTime ModifiedDateTime { get; }
        IAptVerdictContext Clone();
    }
}

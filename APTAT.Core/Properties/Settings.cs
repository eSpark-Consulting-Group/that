using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace APTAT.Core.Properties
{
    public sealed partial class Settings
    {
        public string[] SupportedDnsNamesList
        {
            get
            {
                string supportedDnsNames = SupportedDnsNames;
                if (string.IsNullOrWhiteSpace(supportedDnsNames))
                {
                    Expression expression = (((Expression<Func<Settings, object>>)(s => s.SupportedDnsNames)).Body);
                    string propertyName = ((MemberExpression)expression).Member.Name;
                    throw new InvalidOperationException(String.Format("The setting '{0}' is not specified in the application configuration file.", propertyName));
                }

                string[] dnsNames = supportedDnsNames.Split(';');

                IEnumerable<string> result = from dnsName in dnsNames
                                             where !string.IsNullOrWhiteSpace(dnsName)
                                             select dnsName.Trim();
                return result.ToArray();
            }
        }
    }
}

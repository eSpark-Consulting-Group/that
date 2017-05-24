using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APTAT.Core.CMC.Request
{
    public interface IEmailActionRequest
    {
        EmailAction EmailAction { get; set; }
    }
    public enum EmailAction
    {
        Save,
        Edit
    }
}

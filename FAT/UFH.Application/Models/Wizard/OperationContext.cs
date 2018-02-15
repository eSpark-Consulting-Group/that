using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UFH.Application.Models
{
    public class OperationContext
    {
        public Object UserState
        {
            get;
            set;
        }

        public Action Success
        {
            get;
            set;
        }

        public Object Arg
        {
            get;
            set;
        }
    }
}

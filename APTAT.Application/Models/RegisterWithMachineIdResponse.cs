using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace APTAT.Application.Models
{
    class RegisterWithMachineIdResponse
    {
        public class Rootobject
        {
            public string api_key { get; set; }
            public string result_description { get; set; }
            public string result_message { get; set; }
            public int return_code { get; set; }
        }

    }
}

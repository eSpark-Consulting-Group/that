using System;

namespace DDT.Application.Services
{
    public class GetDomainNameResponse: DomainEventArgs 
    {
        public GetDomainNameResponse(string name, Exception exception)
        {
            Name = name;
            Exception = exception;
        }

        public string Name { get;  }
    }
}

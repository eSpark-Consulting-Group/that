using APTAT.DeploymentCore.Facade.Descent.DataContracts.SearchCriteria.EndpointAddress;

namespace DDT.Application.Models
{
    public abstract class EndpointAddressWrapper : DomainObject<BaseEndpointAddress>
    {
        protected BaseEndpointAddress _address;

        public override BaseEndpointAddress BaseEntity
        {
            get { return _address; }
            set { _address = value; }
        }
    }

    public abstract class EndpointAddressWrapper<T> : EndpointAddressWrapper where T : BaseEndpointAddress
    {
        public T EndpointAddress
        {
            get { return (T) _address; }
        }
    }
}
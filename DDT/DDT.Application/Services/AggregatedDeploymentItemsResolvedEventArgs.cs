using System;
using DDT.Application.Models;

namespace DDT.Application.Services
{
    public class AggregatedDeploymentItemsResolvedEventArgs : DomainEventArgs
    {        
        public DeploymentItem[] ItemsByIp { get;  set; }
        public DeploymentItem[] ItemsByWorkgroup { get; set; }
        public DeploymentItem[] ItemsByDomain { get; set; }
        public AggregatedDeploymentItemsResolvedEventArgs(DeploymentItem[] itemsByIp, DeploymentItem[] itemsByDomain, DeploymentItem[] itemsByWorkgroup,                                                        
                                                        Exception error = null)
        {
            ItemsByIp = itemsByIp;
            ItemsByWorkgroup = itemsByWorkgroup;
            ItemsByDomain = itemsByDomain;            
            Exception = error;
        }

        public AggregatedDeploymentItemsResolvedEventArgs()
        {
            ItemsByIp = new DeploymentItem[] { };
            ItemsByWorkgroup = new DeploymentItem[] { };
            ItemsByDomain = new DeploymentItem[] { };
        }

        public AggregatedDeploymentItemsResolvedEventArgs(Exception error)
        {
            this.Exception = error;
            
        }
    }
}

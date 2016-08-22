using DDT.Application.Models;
using System;
using System.Collections.Generic;

namespace DDT.Application.Services
{
    public class GetByMixedConditionCompletedEventArgs : DomainEventArgs
    {
        public Dictionary<DeploymentItemSource, DeploymentItem[]>  Items { get; private set; }
        
        public GetByMixedConditionCompletedEventArgs(Dictionary<DeploymentItemSource, DeploymentItem[]> items, Exception error = null)
        {
            Items = items;
            Exception = error;
        }
    }
}
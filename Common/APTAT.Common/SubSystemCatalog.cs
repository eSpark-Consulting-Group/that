using APTAT.Common;
using DDT.Core;
using System;
using System.Collections.Generic;

namespace APTAT.Common
{

    public class SubSystemCatalog : ISubSystemCatalog
    {
        private readonly List<ISubSystem> _subSystems = new List<ISubSystem>();
        private readonly IChangeTrackingSubSystem _changeTrackingSubSystem = new ChangeTrackingSubSystem();

        public SubSystemCatalog()
        {
            _subSystems.Add(_changeTrackingSubSystem);
      //      _subSystems.Add(new DescentSubSystem());

            foreach (ISubSystem system in _subSystems)
            {
                system.Init(this);
            }
        }


        public T Find<T>()
        {
            foreach (ISubSystem system in _subSystems)
            {
                if (system is T)
                {
                    return (T) system;
                }
            }
            throw new InvalidOperationException(string.Format(@"Subsystem with type '{0}' not found", typeof(T).Name));
        }

        public void RemoveSessionId(Guid guid)
        {

        }
    }

}
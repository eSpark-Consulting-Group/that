using System;
using System.Collections.Generic;

namespace APTAT.Common
{
    public interface ICompositionHelper
    {
        T Resolve<T>() where T : class;

        void Register<TIService, TService>(Boolean isSingleton)
            where TIService : class
            where TService : class, TIService;

        void Register<TIService, TService>()
            where TIService : class
            where TService : class, TIService;

        void RegisterModules(IEnumerable<Type> moduleTypes);
    }
}
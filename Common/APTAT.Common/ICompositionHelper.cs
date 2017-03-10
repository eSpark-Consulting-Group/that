using System;
using System.Collections.Generic;

namespace APTAT.Common
{
    public interface ICompositionHelper
    {
        void Register<TIService, TService>()
            where TIService : class
            where TService : class, TIService;

        void RegisterSingleton<TIService, TService>()
            where TIService : class
            where TService : class, TIService;
        void RegisterSingleton<TIService>(Func<TIService> createAction)
          where TIService : class;


        void RegisterModules(IEnumerable<Type> moduleTypes);

        T Resolve<T>() where T : class;
        void AllowOverridingRegistrations(bool allow);
        void Verify();
    }
}
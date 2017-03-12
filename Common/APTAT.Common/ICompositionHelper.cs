using System;
using System.Collections.Generic;

namespace APTAT.Common
{
    public interface ICompositionHelper
    {
        void Register<TIService, TService>()
            where TIService : class
            where TService : class, TIService;
        void Register<TIService>(Func<TIService> createAction)
            where TIService : class;
        void Register<TIService1, TIService2, TService>()
            where TIService1 : class
            where TIService2 : class
            where TService : class, TIService1, TIService2;
        void RegisterLazy<TIService, TService>()
            where TIService : class
            where TService : class, TIService;

        void RegisterSingleton<TIService, TService>()
            where TIService : class
            where TService : class, TIService;
        void RegisterSingleton<TIService>(Func<TIService> createAction)
            where TIService : class;
        void RegisterSingleton<TIService1, TIService2, TService>()
            where TIService1 : class
            where TIService2: class
            where TService : class, TIService1, TIService2;
        void RegisterSingletonLazy<TIService, TService>()
            where TIService : class
            where TService : class, TIService;

        void RegisterModules(IEnumerable<Type> moduleTypes);

        T Resolve<T>() where T : class;
        void AllowOverridingRegistrations(bool allow);
        void Verify();
    }
}
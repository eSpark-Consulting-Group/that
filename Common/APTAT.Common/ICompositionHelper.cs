using System;

namespace APTAT.Common
{
    public interface ICompositionHelper
    {
        T Resolve<T>() where T : class;

        void Register<TIService, TService>(Boolean isSingelton)
            where TIService : class
            where TService : class, TIService;

        void Register<TIService, TService>()
            where TIService : class
            where TService : class, TIService;

        TService GetService<TService>() where TService : class;

        void Verify();
    }
}
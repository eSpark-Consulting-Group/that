using System;
using System.Linq;
using SimpleInjector;
using SimpleInjector.Advanced;
using System.Collections.Generic;

namespace APTAT.Common
{
    public class CompositionHelper : ICompositionHelper
    {
        internal static Lifestyle InstancePerDependencyLifestyle { get; } = new InstancePerDependencyLifestyle();

        public enum CreationPolicy { Transient, Scoped, Singleton }

        [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, Inherited = false, AllowMultiple = false)]
        public sealed class CreationPolicyAttribute : Attribute
        {
            public CreationPolicyAttribute(CreationPolicy policy)
            {
                Policy = policy;
            }

            public CreationPolicy Policy { get; private set; }
        }

        public class AttributeBasedLifestyleSelectionBehavior : ILifestyleSelectionBehavior
        {
            private const CreationPolicy DefaultPolicy = CreationPolicy.Transient;

            public Lifestyle SelectLifestyle(Type serviceType, Type implementationType)
            {
                var attribute = implementationType.GetCustomAttributes(typeof(CreationPolicyAttribute), false).FirstOrDefault()
                    ?? serviceType.GetCustomAttributes(typeof(CreationPolicyAttribute), false).FirstOrDefault();

                switch (attribute != null ? ((CreationPolicyAttribute)attribute).Policy : DefaultPolicy)
                {
                    case CreationPolicy.Singleton: return Lifestyle.Singleton;
                    case CreationPolicy.Scoped: return Lifestyle.Scoped;
                    case CreationPolicy.Transient: return Lifestyle.Transient;
                    default: return CompositionHelper.InstancePerDependencyLifestyle; 
                }
            }
        }

        private readonly Container _container;

        public static ICompositionHelper Instance { get; }

        private CompositionHelper()
        {
            _container = new Container();
            _container.Options.LifestyleSelectionBehavior = new AttributeBasedLifestyleSelectionBehavior();
        }

        static CompositionHelper()
        {
            Instance = new CompositionHelper();
        }

        public void Register<TIService, TService>()
            where TIService : class
            where TService : class, TIService
        {
            _container.Register<TIService, TService>(InstancePerDependencyLifestyle);
        }

        public void RegisterSingleton<TIService, TService>()
            where TIService : class
            where TService : class, TIService
        {
            _container.RegisterSingleton<TIService, TService>();
        }

        public void RegisterModules(IEnumerable<Type> moduleTypes)
        {
            using (var container = new Container())
            {
                container.RegisterCollection<IDependencyLocatorModule>(moduleTypes);
                container.RegisterInitializer<IDependencyLocatorModule>(_ => _.RegisterDependencies(this));
                container.Verify();
            }
        }

        public void Verify()
        {
          
            _container.Verify();
        }

        public T Resolve<T>() where T : class
        {
            return _container.GetInstance<T>();
        }

        public void AllowOverridingRegistrations(bool allow)
        {
            _container.Options.AllowOverridingRegistrations = allow;
        }
    }
}
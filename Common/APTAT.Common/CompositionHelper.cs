using System;
using System.Linq;
using System.Reflection;
using SimpleInjector;
using SimpleInjector.Advanced;
using System.Collections.Generic;

namespace APTAT.Common
{
    public class CompositionHelper : ICompositionHelper
    {
        internal static Lifestyle InstancePerDependencyLifestyle { get; } = new InstancePerDependencyLifestyle();
        public class MostResolvableConstructorBehavior : IConstructorResolutionBehavior
        {
            private readonly Container _container;

            public MostResolvableConstructorBehavior(Container container)
            {
                _container = container;
            }

            private bool IsCalledDuringRegistrationPhase
            {
                get { return !_container.IsLocked(); }
            }

            public ConstructorInfo GetConstructor(Type service, Type implementation)
            {
                var constructors = implementation.GetConstructors();
                return (
                    from ctor in constructors
                    let parameters = ctor.GetParameters()
                    where IsCalledDuringRegistrationPhase
                        || constructors.Length == 1
                        || parameters.All(p => CanBeResolved(p, service, implementation))
                    orderby parameters.Length descending
                    select ctor)
                    .First();
            }

            private bool CanBeResolved(ParameterInfo p, Type service, Type implementation)
            {
                return _container.GetRegistration(p.ParameterType) != null ||
                    CanBuildType(p, service, implementation);
            }

            private bool CanBuildType(ParameterInfo p, Type service, Type implementation)
            {
                try
                {
                    _container.Options.DependencyInjectionBehavior.BuildExpression(
                        new InjectionConsumerInfo(service, implementation, p));
                    return true;
                }
                catch (ActivationException)
                {
                    return false;
                }
            }
        }

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
            _container.Options.ConstructorResolutionBehavior = new MostResolvableConstructorBehavior(_container);
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
                _container.Verify();
            }
        }

        public T Resolve<T>() where T : class
        {
            return _container.GetInstance<T>();
        }
    }
}
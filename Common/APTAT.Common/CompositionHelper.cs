using System;
using System.Linq;
using System.Reflection;
using SimpleInjector;
using SimpleInjector.Advanced;

namespace APTAT.Common
{
    public class CompositionHelper : IServiceProvider, ICompositionHelper
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
        public static void Initialize(bool autoLoadModules = true)
        {
            if (autoLoadModules)
            {
                // Find all assemblies and all classes implementing the IDependencyLocatorModule
                // for each of these, register the dependencies
                var assemblies = AppDomain.CurrentDomain.GetAssemblies().Where( a => a.FullName.StartsWith("APT") ||
                    a.FullName.StartsWith("DDT"));
                foreach (var assembly in assemblies)
                {
                    var q = from t in assembly.GetTypes()
                            where t.IsClass && typeof(IDependencyLocatorModule).IsAssignableFrom(t)
                            select t;
                    q.ToList().ForEach(t => LoadModule(t, Instance));
                }
            }
        }
        private static void LoadModule(Type type, ICompositionHelper resolver)
        {
            ConstructorInfo constructorInfo = type.GetConstructor(Type.EmptyTypes);
            if (constructorInfo == null)
            {
                // Log an error
            }
            else
            {
                var locatorModule = constructorInfo.Invoke(new object[] { }) as IDependencyLocatorModule;
                if (locatorModule == null)
                {
                    // Could not instantiate the locator module, log an error and continue
                }
                else
                {
                    locatorModule.RegisterDependencies(resolver);
                }
            }
        }

        public object GetService(Type serviceType)
        {
            IServiceProvider serviceProvider = _container;

            return serviceProvider.GetService(serviceType);
        }

        public TService GetService<TService>() where TService : class
        {
            IServiceProvider serviceProvider = _container;

            return serviceProvider.GetService(typeof(TService)) as TService;
        }

        public void Register<TService>() where TService : class
        {
            _container.Register<TService>();
        }

        public void Register<TIService, TService>()
            where TIService : class
            where TService : class, TIService
        {
            _container.Register<TIService, TService>();
        }

        public void Register<TIService, TService>(Boolean isSingelton)
            where TIService : class
            where TService : class, TIService
        {
            if (isSingelton)
                _container.Register<TIService, TService>(Lifestyle.Singleton);
            else
                _container.Register<TIService, TService>(InstancePerDependencyLifestyle);
        }

        public T Resolve<T>() where T:class
        {
            return _container.GetInstance<T>();
        }
        

        public void Verify()
        {
            _container.Verify();
        }
    }
}
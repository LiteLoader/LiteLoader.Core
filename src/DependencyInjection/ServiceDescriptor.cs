using System;

namespace LiteLoader.DependencyInjection
{
    internal sealed class ServiceDescriptor
    {
        public object Implementation { get; set; }
        public Func<IServiceProvider, object> ImplementationFactory { get; }
        public Type ImplementationType { get; }
        public bool IsTransient { get; }
        public Type ServiceType { get; }

        public ServiceDescriptor(Type serviceType, Type implementationType, Func<IServiceProvider, object> factory, object implementation, bool isTransient)
        {
            ServiceType = serviceType;
            ImplementationType = implementationType;
            Implementation = implementation;
            ImplementationFactory = factory;
            IsTransient = isTransient;
        }
    }
}

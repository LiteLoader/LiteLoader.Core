using System;
using System.Collections.Generic;

namespace LiteLoader.DependencyInjection
{
    public sealed class DynamicServiceProvider : IDynamicServiceProvider
    {
        private readonly Type _iServiceType = typeof(IServiceProvider);

        private readonly Dictionary<Type, ServiceDescriptor> _services = new Dictionary<Type, ServiceDescriptor>();

        public void AddService(Type serviceType, Type implementationType, Func<IServiceProvider, object> implementationFactory, bool isTransient, object implementation)
        {
            if (implementationType == null && implementation != null)
            {
                implementationType = implementation.GetType();
            }

            if (serviceType == null && implementationType != null)
            {
                serviceType = implementationType;
            }

            lock (_services)
            {
                if (_services.ContainsKey(serviceType))
                {
                    throw new InvalidOperationException($"Service Provider already contains the service {serviceType.FullName}");
                }

                if (implementationFactory == null && implementation == null)
                {
                    implementationFactory = ActivationUtility.CreateFactory(implementationType);
                }

                ServiceDescriptor descriptor = new ServiceDescriptor(serviceType, implementationType, implementationFactory, implementation, isTransient);
                _services[serviceType] = descriptor;
            }
        }

        public object GetService(Type serviceType)
        {
            if (serviceType == null)
            {
                throw new ArgumentNullException(nameof(serviceType));
            }

            if (_iServiceType.IsAssignableFrom(serviceType))
            {
                return this;
            }

            ServiceDescriptor descriptor;
            lock (_services)
            {
                if (!_services.TryGetValue(serviceType, out descriptor))
                {

                    foreach (KeyValuePair<Type, ServiceDescriptor> d in _services)
                    {
                        if (d.Key.IsAssignableFrom(serviceType))
                        {
                            descriptor = d.Value;
                            break;
                        }
                    }

                    if (descriptor == null)
                    {
                        return null;
                    }
                }
            }

            if (descriptor.IsTransient)
            {
                return descriptor.ImplementationFactory(this);
            }

            if (descriptor.Implementation == null)
            {
                descriptor.Implementation = descriptor.ImplementationFactory(this);
            }

            return descriptor.Implementation;
        }

        public bool RemoveService(Type serviceType)
        {
            if (serviceType == null)
            {
                return false;
            }

            lock (_services)
            {
                if (_services.TryGetValue(serviceType, out ServiceDescriptor descriptor))
                {
                    if (descriptor.Implementation != null && descriptor.Implementation is IDisposable dispose)
                    {
                        try
                        {
                            dispose.Dispose();
                        }
                        catch
                        {
                        }
                    }
                    return _services.Remove(serviceType);
                }
            }

            return false;
        }
    }
}

using System;
using GroboContainer.Core;
using GroboContainer.Extensions.DependencyInjection.Registries;
using GroboContainer.Impl.Exceptions;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace GroboContainer.Extensions.DependencyInjection
{
    public class GroboServiceProvider : IServiceProvider, IDisposable
    {
        public GroboServiceProvider(IContainer container)
        {
            this.container = container ?? throw new ArgumentNullException(nameof(container));
            serviceLifetimeRegistry = container.Get<ServiceLifetimeRegistry>();
            serviceFactoryRegistry = container.Get<ServiceFactoryRegistry>();
        }

        [NotNull]
        public object GetService(Type serviceType)
        {
            var lifetime = serviceLifetimeRegistry.Resolve(serviceType);
            switch (lifetime)
            {
                case ServiceLifetime.Singleton:
                {
                    if (serviceFactoryRegistry.TryPop(serviceType, out var factoryFunc))
                    {
                        var service = factoryFunc(container);
                        container.Configurator.ForAbstraction(serviceType).UseInstances(service);
                        return service;
                    }

                    return container.Get(serviceType);
                }
                case ServiceLifetime.Scoped:
                case ServiceLifetime.Transient:
                {
                    if (serviceFactoryRegistry.TryResolve(serviceType, out var factoryFunc))
                        return factoryFunc(container);
                    return container.Create(serviceType);
                }
                default:
                    throw new ContainerException($"Unknown ServiceLifetime: {lifetime}", null);
            }
        }

        public void Dispose()
        {
            container.Dispose();
        }

        private readonly IContainer container;
        private readonly ServiceFactoryRegistry serviceFactoryRegistry;
        private readonly ServiceLifetimeRegistry serviceLifetimeRegistry;

    }
}
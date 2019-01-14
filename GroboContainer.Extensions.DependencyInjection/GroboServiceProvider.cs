using System;
using System.Collections.Generic;
using GroboContainer.Core;
using GroboContainer.Extensions.DependencyInjection.Registries;
using GroboContainer.Impl.Exceptions;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace GroboContainer.Extensions.DependencyInjection
{
    public class GroboServiceProvider : IServiceProvider, IDisposable
    {
        public GroboServiceProvider([NotNull] IContainer container, [NotNull] IEnumerable<ServiceDescriptor> serviceDescriptors)
        {
            this.container = container ?? throw new ArgumentNullException(nameof(container));
            serviceLifetimeRegistry = new ServiceLifetimeRegistry();
            serviceFactoryRegistry = new ServiceFactoryRegistry();

            ConfigureContainer();

            Populate(serviceDescriptors);
        }

        private GroboServiceProvider([NotNull] IContainer container, [NotNull] ServiceLifetimeRegistry serviceLifetimeRegistry, [NotNull] ServiceFactoryRegistry serviceFactoryRegistry)
        {
            this.container = container;
            this.serviceLifetimeRegistry = serviceLifetimeRegistry;
            this.serviceFactoryRegistry = serviceFactoryRegistry;

            ConfigureContainer();
        }

        private void ConfigureContainer()
        {
            container.Configurator.ForAbstraction<IServiceProvider>().UseInstances(this);
            container.Configurator.ForAbstraction<GroboServiceProvider>().UseInstances(this);
        }

        internal GroboServiceProvider MakeChildServiceProvider()
        {
            return new GroboServiceProvider(container.MakeChildContainer(), serviceLifetimeRegistry, serviceFactoryRegistry);
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

        private void Populate([NotNull] IEnumerable<ServiceDescriptor> serviceDescriptors)
        {
            container.Configurator.ForAbstraction<IServiceProvider>().UseType<GroboServiceProvider>();
            container.Configurator.ForAbstraction<IServiceScopeFactory>().UseType<GroboServiceScopeFactory>();

            foreach (var descriptor in serviceDescriptors)
            {
                serviceLifetimeRegistry.Register(descriptor.ServiceType, descriptor.Lifetime);
                var abstractionConfigurator = container.Configurator.ForAbstraction(descriptor.ServiceType);

                if (descriptor.ImplementationType != null)
                    abstractionConfigurator.UseType(descriptor.ImplementationType);
                else if (descriptor.ImplementationFactory != null)
                {
                    serviceFactoryRegistry.Register(descriptor.ServiceType, c =>
                    {
                        var serviceProvider = c.Get<GroboServiceProvider>();
                        return descriptor.ImplementationFactory(serviceProvider);
                    });
                }
                else
                    abstractionConfigurator.UseInstances(descriptor.ImplementationInstance);
            }
        }

        private readonly IContainer container;
        private readonly ServiceFactoryRegistry serviceFactoryRegistry;
        private readonly ServiceLifetimeRegistry serviceLifetimeRegistry;
    }
}
using System;
using System.Collections.Generic;
using GroboContainer.Core;
using GroboContainer.Extensions.DependencyInjection.Registries;
using Microsoft.Extensions.DependencyInjection;

namespace GroboContainer.Extensions.DependencyInjection
{
    public static class GroboRegistrationExtensions
    {
        public static void Populate(this IContainer container, IEnumerable<ServiceDescriptor> serviceDescriptors)
        {
            container.Configurator.ForAbstraction<IServiceProvider>().UseType<GroboServiceProvider>();
            container.Configurator.ForAbstraction<IServiceScopeFactory>().UseType<GroboServiceScopeFactory>();

            var serviceLifetimeRegistry = new ServiceLifetimeRegistry();
            var serviceFactoryRegistry = new ServiceFactoryRegistry();
            container.Configurator.ForAbstraction<ServiceLifetimeRegistry>().UseInstances(serviceLifetimeRegistry);
            container.Configurator.ForAbstraction<ServiceFactoryRegistry>().UseInstances(serviceFactoryRegistry);

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
                        var serviceProvider = c.Get<IServiceProvider>();
                        return descriptor.ImplementationFactory(serviceProvider);
                    });
                }
                else
                    abstractionConfigurator.UseInstances(descriptor.ImplementationInstance);
            }
        }
    }
}
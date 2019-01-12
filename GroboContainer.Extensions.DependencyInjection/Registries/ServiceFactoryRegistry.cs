using System;
using GroboContainer.Core;

namespace GroboContainer.Extensions.DependencyInjection.Registries
{
    internal class ServiceFactoryRegistry : ServiceRegistry<Func<IContainer, object>>
    {
    }
}
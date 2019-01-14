using System;
using System.Collections.Concurrent;
using GroboContainer.Impl.Exceptions;
using JetBrains.Annotations;

namespace GroboContainer.Extensions.DependencyInjection.Registries
{
    internal abstract class ServiceRegistry<TValue>
    {
        public void Register([NotNull] Type serviceType, [NotNull] TValue value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            if (!registrations.TryAdd(serviceType, value))
                throw new ContainerException($"Service {serviceType.FullName} already registered", null);
        }

        public bool TryPop([NotNull] Type serviceType, [NotNull] out TValue value)
        {
            return registrations.TryRemove(serviceType, out value);
        }

        [NotNull]
        public TValue Resolve([NotNull] Type serviceType)
        {
            if (registrations.TryGetValue(serviceType, out var result))
                return result;
            throw new ContainerException($"Service {serviceType.FullName} was not registered", null);
        }

        public bool TryResolve([NotNull] Type serviceType, [NotNull] out TValue result)
        {
            return registrations.TryGetValue(serviceType, out result);
        }


        private readonly ConcurrentDictionary<Type, TValue> registrations = new ConcurrentDictionary<Type, TValue>();
    }
}
using GroboContainer.Core;
using Microsoft.Extensions.DependencyInjection;

namespace GroboContainer.Extensions.DependencyInjection
{
    public class GroboServiceScopeFactory : IServiceScopeFactory
    {
        public GroboServiceScopeFactory(IContainer container)
        {
            this.container = container;
        }

        public IServiceScope CreateScope()
        {
            return new GroboServiceScope(new GroboServiceProvider(container.MakeChildContainer()));
        }

        private readonly IContainer container;
    }
}
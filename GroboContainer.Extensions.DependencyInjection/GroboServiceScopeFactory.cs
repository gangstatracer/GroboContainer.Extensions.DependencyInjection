using Microsoft.Extensions.DependencyInjection;

namespace GroboContainer.Extensions.DependencyInjection
{
    public class GroboServiceScopeFactory : IServiceScopeFactory
    {
        public GroboServiceScopeFactory(GroboServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public IServiceScope CreateScope()
        {
            return new GroboServiceScope(serviceProvider.MakeChildServiceProvider());
        }

        private readonly GroboServiceProvider serviceProvider;
    }
}
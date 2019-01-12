using System;
using Microsoft.Extensions.DependencyInjection;

namespace GroboContainer.Extensions.DependencyInjection
{
    public class GroboServiceScope : IServiceScope
    {
        public GroboServiceScope(GroboServiceProvider groboServiceProvider)
        {
            this.groboServiceProvider = groboServiceProvider;
        }

        public void Dispose()
        {
            groboServiceProvider.Dispose();
        }

        public IServiceProvider ServiceProvider => groboServiceProvider;

        private readonly GroboServiceProvider groboServiceProvider;
    }
}
using System;
using System.Reflection;
using GroboContainer.Core;
using GroboContainer.Impl;
using NUnit.Framework;

namespace GroboContainer.Extensions.DependencyInjection.Tests
{
    public class GroboServiceProviderTests
    {
        [Test]
        public void TestRegisterSingletonImplementationInstance()
        {
            var container = new Container(new ContainerConfiguration(new Assembly[0]));
        }

        private interface IService
        {
            int GetValue();
        }

        private class Service : IService
        {
            public Service(int value)
            {
                this.value = value;
            }

            public int GetValue() => value;

            private readonly int value;
        }
    }
}
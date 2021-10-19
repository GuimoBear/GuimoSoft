using DeepEqual.Syntax;
using FluentAssertions;
using GuimoSoft.Bus.Core.Logs;
using GuimoSoft.Bus.Core.Logs.Builder;
using Moq;
using System;
using Xunit;

namespace GuimoSoft.Bus.Tests.Core.Logs
{
    public class BusLogDispatcherTests
    {
        [Fact]
        public void ConstructorShouldCreateBusLogDispatcher()
        {
            var sut = new BusLogDispatcher(Mock.Of<IServiceProvider>());
            Assert.IsType<BusLogDispatcher>(sut);
        }

        [Fact]
        public void ConstructorShouldThrowIfAnyParameterIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new BusLogDispatcher(null));
        }


        [Fact]
        public void FromBusFacts()
        {
            var serviceProvider = Mock.Of<IServiceProvider>();

            var expectedLogBuilder = new BusLogDispatcherBuilder(serviceProvider, Bus.Abstractions.BusName.Kafka);

            var sut = new BusLogDispatcher(serviceProvider);

            var builder = sut.FromBus(Bus.Abstractions.BusName.Kafka);

            builder
                .Should().BeOfType<BusLogDispatcherBuilder>();

            builder.WithDeepEqual(expectedLogBuilder)
                .Assert();
        }
    }
}

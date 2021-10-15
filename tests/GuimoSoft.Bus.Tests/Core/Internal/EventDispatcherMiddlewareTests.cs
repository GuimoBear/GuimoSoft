using FluentAssertions;
using GuimoSoft.Bus.Abstractions;
using GuimoSoft.Bus.Core.Internal;
using GuimoSoft.Bus.Tests.Fakes;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace GuimoSoft.Bus.Tests.Core.Internal
{
    public class EventDispatcherMiddlewareTests
    {
        [Fact]
        public void InvokeAsyncShouldBeExecutedWithoudAnyProblem()
        {
            var services = new ServiceCollection();
            services.AddSingleton<FakeEventnHandler>();
            using var serviceProvider = services.BuildServiceProvider(true);

            var consumeContext = new ConsumeContext<FakeEvent>(new FakeEvent("", ""), serviceProvider, new ConsumeInformations(BusName.None, ServerName.Default, ""), CancellationToken.None);

            lock (Utils.Lock)
            {
                Utils.ResetarSingletons();
                Singletons.GetAssemblies().Add(typeof(FakeEvent).Assembly);
                var middleware = new EventDispatcherMiddleware<FakeEvent>();

                middleware.HandlerTypes
                    .Should().HaveCount(1);

                middleware.HandlerTypes.First()
                    .Should().Be(typeof(FakeEventnHandler));

                bool executed = false;

                middleware.InvokeAsync(consumeContext, () => 
                {
                    executed = true;
                    return Task.CompletedTask;
                }).Wait();

                executed
                    .Should().BeTrue();

                Utils.ResetarSingletons();
            }
        }
    }
}

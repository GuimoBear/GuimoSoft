using FluentAssertions;
using GuimoSoft.Bus.Abstractions;
using GuimoSoft.Bus.Core.Internal;
using GuimoSoft.Bus.Core.Internal.Middlewares;
using GuimoSoft.Bus.Tests.Fakes;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace GuimoSoft.Bus.Tests.Core.Internal
{
    public class EventDispatcherMiddlewareTests
    {
        [Fact]
        public void ConstructWithFakeEventShouldBeRegisterFakeEventHandlerAsTheUniqueHandler()
        {
            lock (Utils.Lock)
            {
                Utils.ResetarSingletons();
                new ServiceCollection().RegisterMediatorFromNewAssemblies(new List<Assembly> { typeof(ChildFakeEvent).Assembly });
                var middleware = new EventDispatcherMiddleware<FakeEvent>();

                middleware.HandlerTypes
                    .Should().HaveCount(1);

                middleware.HandlerTypes.First()
                    .Should().Be(typeof(FakeEventHandler));
            }
        }

        [Fact]
        public void ConstructWithChildFakeEventShouldBeRegisterChildFakeEventHandlerAndFakeEventHandler()
        {
            lock (Utils.Lock)
            {
                Utils.ResetarSingletons();
                new ServiceCollection().RegisterMediatorFromNewAssemblies(new List<Assembly> { typeof(ChildFakeEvent).Assembly });

                var middleware = new EventDispatcherMiddleware<ChildFakeEvent>();

                middleware.HandlerTypes
                    .Should().HaveCount(2);

                middleware.HandlerTypes
                    .Should().Contain(typeof(ChildFakeEventHandler));

                middleware.HandlerTypes
                    .Should().Contain(typeof(FakeEventHandler));
            }
        }

        [Fact]
        public void InvokeAsyncWithFakeEventShouldBeExecutedWithoudAnyProblem()
        {
            var services = new ServiceCollection();
            services.AddSingleton<FakeEventHandler>();
            using var serviceProvider = services.BuildServiceProvider(true);

            var consumeContext = new ConsumeContext<FakeEvent>(new FakeEvent("", ""), serviceProvider, new ConsumeInformations(BusName.None, ServerName.Default, ""), CancellationToken.None);

            lock (Utils.Lock)
            {
                Utils.ResetarSingletons();
                Singletons.GetAssemblies().Add(typeof(FakeEvent).Assembly);
                var middleware = new EventDispatcherMiddleware<FakeEvent>();

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


        [Fact]
        public void InvokeAsyncWithChildFakeEventShouldBeExecutedWithoudAnyProblem()
        {
            var services = new ServiceCollection();
            services.AddSingleton<FakeEventHandler>();
            services.AddSingleton<ChildFakeEventHandler>();
            using var serviceProvider = services.BuildServiceProvider(true);

            var consumeContext = new ConsumeContext<ChildFakeEvent>(new ChildFakeEvent("", "", ""), serviceProvider, new ConsumeInformations(BusName.None, ServerName.Default, ""), CancellationToken.None);

            lock (Utils.Lock)
            {
                Utils.ResetarSingletons();
                Singletons.GetAssemblies().Add(typeof(ChildFakeEvent).Assembly);
                var middleware = new EventDispatcherMiddleware<ChildFakeEvent>();

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

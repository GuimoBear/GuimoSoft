using FluentAssertions;
using GuimoSoft.Bus.Abstractions;
using GuimoSoft.Bus.Core;
using GuimoSoft.Bus.Tests.Fakes;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace GuimoSoft.Bus.Tests.Consumer
{
    public class PipelineTests
    {
        private readonly IServiceProvider services;
        private readonly Pipeline pipeline;

        public PipelineTests()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<FakePipelineMessageMiddlewareOne>();
            serviceCollection.AddSingleton<FakePipelineMessageMiddlewareTwo>();
            serviceCollection.AddSingleton<FakePipelineMessageMiddlewareThree>();
            services = serviceCollection.BuildServiceProvider();
            var middlewareTypes = new List<Type>
            {
                typeof(FakePipelineMessageMiddlewareOne),
                typeof(FakePipelineMessageMiddlewareTwo),
                typeof(FakePipelineMessageMiddlewareThree)
            };
            pipeline = new Pipeline(middlewareTypes);
        }

        [Fact]
        public void ConstructiorWithNullParameterShouldBeThrowArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new Pipeline(null));
        }

        [Fact]
        public async Task ExecuteShouldBeExecutedWithoutExceptions()
        {
            var message = new FakePipelineMessage();
            using var scope = services.CreateScope();
            var context = await pipeline.Execute(message.GetType(), message, scope.ServiceProvider) as ConsumptionContext<FakePipelineMessage>;

            context
                .Should().NotBeNull();

            context.Items
                .Should().NotBeNull().And.HaveCount(3);

            context.Items.Should().ContainKey(FakePipelineMessageMiddlewareOne.Name);
            context.Items.Should().ContainKey(FakePipelineMessageMiddlewareTwo.Name);
            context.Items.Should().ContainKey(FakePipelineMessageMiddlewareThree.Name);

            context.Message
                .Should().NotBeNull();

            context.Message.MiddlewareNames
                .Should().NotBeNull().And.HaveCount(3);

            context.Message.MiddlewareNames[0]
                .Should().Be(FakePipelineMessageMiddlewareOne.Name);

            context.Message.MiddlewareNames[1]
                .Should().Be(FakePipelineMessageMiddlewareTwo.Name);
            context.Message.MiddlewareNames[2]
                .Should().Be(FakePipelineMessageMiddlewareThree.Name);
        }


        [Fact]
        public async Task ExecuteWithExecutionStopInMiddlewareTwoShouldBeExecutedWithoutExceptions()
        {
            var message = new FakePipelineMessage(FakePipelineMessageMiddlewareTwo.Name);
            using var scope = services.CreateScope();

            var context = await pipeline.Execute(message.GetType(), message, scope.ServiceProvider) as ConsumptionContext<FakePipelineMessage>;

            message
                .MiddlewareNames.Should().NotBeNull().And.HaveCount(2);

            message.MiddlewareNames[0]
                .Should().Be(FakePipelineMessageMiddlewareOne.Name);

            message.MiddlewareNames[1]
                .Should().Be(FakePipelineMessageMiddlewareTwo.Name);
        }
    }
}

using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GuimoSoft.Bus.Abstractions;
using GuimoSoft.Bus.Core.Internal;
using GuimoSoft.Bus.Tests.Fakes;
using Xunit;

namespace GuimoSoft.Bus.Tests.Consumer
{
    public class PipelineTests
    {
        public static readonly IEnumerable<object[]> ConstructorInvalidData
            = new List<object[]>
            {
                new object[] { new List<Type>(), typeof(PipelineTests) },
                new object[] { new List<Type> { typeof(FakePipelineEventMiddlewareOne), typeof(FakePipelineEventMiddlewareTwo), typeof(FakePipelineEventMiddlewareThree), typeof(FakeEventMiddleware) }, typeof(FakePipelineEvent) },
            };

        private static readonly IReadOnlyDictionary<string, string> EMPTY_HEADER = new Dictionary<string, string>();

        private readonly IServiceProvider services;
        private readonly Pipeline pipeline;

        public PipelineTests()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<FakePipelineEventMiddlewareOne>();
            serviceCollection.AddSingleton<FakePipelineEventMiddlewareTwo>();
            serviceCollection.AddSingleton<FakePipelineEventMiddlewareThree>();
            services = serviceCollection.BuildServiceProvider();
            var middlewareTypes = new List<Type>
            {
                typeof(FakePipelineEventMiddlewareOne),
                typeof(FakePipelineEventMiddlewareTwo),
                typeof(FakePipelineEventMiddlewareThree)
            };
            pipeline = new Pipeline(middlewareTypes, typeof(FakePipelineEvent));
        }

        [Fact]
        public void ConstructorWithNullParametersShouldBeThrowArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new Pipeline(null, default));
        }

        [Theory]
        [MemberData(nameof(ConstructorInvalidData))]
        public void ConstructorWithInvalidParametersShouldBeThrowArgumentException(IReadOnlyList<Type> middlewareTypes, Type eventType)
        {
            Assert.Throws<ArgumentException>(() => new Pipeline(middlewareTypes, eventType));
        }

        [Fact]
        public async Task ExecuteShouldBeExecutedWithoutExceptions()
        {
            var @event = new FakePipelineEvent();
            using var scope = services.CreateScope();
            var context = await pipeline.Execute(@event, scope.ServiceProvider, new ConsumeInformations(BusName.Kafka, FakeServerName.FakeHost2, "e"), CancellationToken.None) as ConsumeContext<FakePipelineEvent>;

            context
                .Should().NotBeNull();

            context.Items
                .Should().NotBeNull().And.HaveCount(3);

            context.Items.Should().ContainKey(FakePipelineEventMiddlewareOne.Name);
            context.Items.Should().ContainKey(FakePipelineEventMiddlewareTwo.Name);
            context.Items.Should().ContainKey(FakePipelineEventMiddlewareThree.Name);

            context.Event
                .Should().NotBeNull();

            context.Event.MiddlewareNames
                .Should().NotBeNull().And.HaveCount(3);

            context.Event.MiddlewareNames[0]
                .Should().Be(FakePipelineEventMiddlewareOne.Name);

            context.Event.MiddlewareNames[1]
                .Should().Be(FakePipelineEventMiddlewareTwo.Name);
            context.Event.MiddlewareNames[2]
                .Should().Be(FakePipelineEventMiddlewareThree.Name);
        }


        [Fact]
        public async Task ExecuteWithExecutionStopInMiddlewareTwoShouldBeExecutedWithoutExceptions()
        {
            var @event = new FakePipelineEvent(FakePipelineEventMiddlewareTwo.Name);
            using var scope = services.CreateScope();

            var context = await pipeline.Execute(@event, scope.ServiceProvider, new ConsumeInformations(BusName.Kafka, ServerName.Default, "e"), CancellationToken.None) as ConsumeContext<FakePipelineEvent>;

            @event
                .MiddlewareNames.Should().NotBeNull().And.HaveCount(2);

            @event.MiddlewareNames[0]
                .Should().Be(FakePipelineEventMiddlewareOne.Name);

            @event.MiddlewareNames[1]
                .Should().Be(FakePipelineEventMiddlewareTwo.Name);
        }
    }
}

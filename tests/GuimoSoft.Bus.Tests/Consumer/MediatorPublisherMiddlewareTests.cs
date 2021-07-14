using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using GuimoSoft.Bus.Abstractions;
using GuimoSoft.Bus.Abstractions.Consumer;
using GuimoSoft.Bus.Core;
using GuimoSoft.Bus.Kafka.Consumer;
using GuimoSoft.Bus.Tests.Fakes;
using Xunit;

namespace GuimoSoft.Bus.Tests.Consumer
{
    public class MediatorPublisherMiddlewareTests
    {
        [Fact]
        public async Task When_ExecuteWithoutMediatRConfigured_Then_ThrowException()
        {
            var services = new ServiceCollection();
            var serviceProvider = services.BuildServiceProvider();
            var message = new FakeMessage("test", "");
            var context = new ConsumptionContext<FakeMessage>(message, serviceProvider);

            var sut = new MediatorPublisherMiddleware<FakeMessage>();

            await Assert.ThrowsAsync<InvalidOperationException>(() => sut.InvokeAsync(context, () => Task.CompletedTask));
        }

        [Fact]
        public async Task When_ExecuteWithMediatRConfigured_Then_CallsMediatRPublishMethod()
        {
            var services = new ServiceCollection();

            var message = new FakeMessage("test", "");

            var messageNotification = new MessageNotification<FakeMessage>(message);
            messageNotification.GetHashCode();
            var moqMediatr = new Mock<IMediator>();
            moqMediatr
                .Setup(x => x.Publish(messageNotification, It.IsAny<CancellationToken>()))
                .Verifiable();

            services.AddSingleton(moqMediatr.Object);

            var serviceProvider = services.BuildServiceProvider();
            var context = new ConsumptionContext<FakeMessage>(message, serviceProvider);

            var sut = new MediatorPublisherMiddleware<FakeMessage>();

            await sut.InvokeAsync(context, () => Task.CompletedTask);

            moqMediatr.Verify(x => x.Publish(messageNotification, It.IsAny<CancellationToken>()), Times.Once);

            context.GetMessage()
                .Should().BeEquivalentTo(message);
        }
    }
}

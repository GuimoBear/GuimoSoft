using Moq;
using System.Threading;
using System.Threading.Tasks;
using GuimoSoft.Bus.Kafka.Consumer;
using Xunit;
using System;
using FluentAssertions;

namespace GuimoSoft.Bus.Tests.Consumer
{
    public class KafkaConsumerEventHandlerTests
    {
        [Fact]
        public void Given_InvalidParameter_When_Construct_Then_ThrowArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new KafkaConsumerEventHandler(null));
        }

        [Fact]
        public async Task When_StartAsync_Then_StartConsumersHasCalled()
        {
            var moqKafkaEventConsumerManager = new Mock<IKafkaEventConsumerManager>();
            using var cts = new CancellationTokenSource();

            using var sut = new KafkaConsumerEventHandler(moqKafkaEventConsumerManager.Object);

            await sut.StartAsync(cts.Token);
            await Task.Delay(50);

            moqKafkaEventConsumerManager
                .Verify(x => x.StartConsumers(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public void When_StopAsync_Then_StartConsumersHasCalled()
        {
            using var sut = new KafkaConsumerEventHandler(Mock.Of<IKafkaEventConsumerManager>());

            using var cts = new CancellationTokenSource();

            sut.StopAsync(cts.Token)
                .Should().BeSameAs(Task.CompletedTask);
        }
    }
}

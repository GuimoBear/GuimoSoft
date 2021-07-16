using Confluent.Kafka;
using GuimoSoft.Bus.Kafka.Producer;
using GuimoSoft.Bus.Tests.Fakes;
using GuimoSoft.Core.Serialization;
using GuimoSoft.Core.Serialization.Interfaces;
using Moq;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace GuimoSoft.Bus.Tests.Producer
{
    public class KafkaMessageProducerTests
    {
        [Fact]
        public async Task ProduceShouldThrowInvalidOperationExceptionIfMessageClassNoContainsMessageTopicAttribute()
        {
            var stubMessageProducerBuilder = new Mock<IKafkaProducerBuilder>();
            var mockProducer = new Mock<IProducer<string, byte[]>>();
            stubMessageProducerBuilder
                .Setup(x => x.Build())
                .Returns(mockProducer.Object);
            var fakeMessage = new FakeMessageWithoutMessageTopic("some-key-id", "some-property-value");

            var moqSerializerManager = new Mock<IMessageSerializerManager>();
            moqSerializerManager
                .Setup(x => x.GetSerializer(typeof(FakeMessageWithoutMessageTopic)))
                .Returns(JsonMessageSerializer.Instance);

            var sut = new KafkaMessageProducer(stubMessageProducerBuilder.Object, moqSerializerManager.Object);
            await Assert.ThrowsAsync<InvalidOperationException>(() => sut.ProduceAsync(fakeMessage.Key, fakeMessage, CancellationToken.None));

            moqSerializerManager
                .Verify(x => x.GetSerializer(typeof(FakeMessage)), Times.Never);

            mockProducer
                .Verify(x => x.ProduceAsync(It.IsAny<string>(), It.IsAny<Message<string, byte[]>>(), CancellationToken.None), Times.Never);
        }

        [Fact]
        public async Task ProduceShouldProduceMessageWithCorrectTopic()
        {
            const string expectedTopic = FakeMessage.TOPIC_NAME;
            var stubMessageProducerBuilder = new Mock<IKafkaProducerBuilder>();
            var mockProducer = new Mock<IProducer<string, byte[]>>();
            stubMessageProducerBuilder
                .Setup(x => x.Build())
                .Returns(mockProducer.Object);
            var fakeMessage = new FakeMessage("some-key-id", "some-property-value");

            var moqSerializerManager = new Mock<IMessageSerializerManager>();
            moqSerializerManager
                .Setup(x => x.GetSerializer(typeof(FakeMessage)))
                .Returns(JsonMessageSerializer.Instance);

            var sut = new KafkaMessageProducer(stubMessageProducerBuilder.Object, moqSerializerManager.Object);
            await sut.ProduceAsync(fakeMessage.Key, fakeMessage, CancellationToken.None);

            moqSerializerManager
                .Verify(x => x.GetSerializer(typeof(FakeMessage)), Times.Once);

            mockProducer.Verify(x => x.ProduceAsync(expectedTopic,
                It.IsAny<Message<string, byte[]>>(),
                CancellationToken.None));
        }

        [Fact]
        public async Task ProduceShouldProduceMessageWithCorrectKey()
        {
            const string expectedMessageKey = "some-key-id";
            var stubMessageProducerBuilder = new Mock<IKafkaProducerBuilder>();
            var mockProducer = new Mock<IProducer<string, byte[]>>();
            stubMessageProducerBuilder
                .Setup(x => x.Build())
                .Returns(mockProducer.Object);
            var fakeMessage = new FakeMessage(expectedMessageKey, "some-property-value");

            var moqSerializerManager = new Mock<IMessageSerializerManager>();
            moqSerializerManager
                .Setup(x => x.GetSerializer(typeof(FakeMessage)))
                .Returns(JsonMessageSerializer.Instance);

            var sut = new KafkaMessageProducer(stubMessageProducerBuilder.Object, moqSerializerManager.Object);
            await sut.ProduceAsync(expectedMessageKey, fakeMessage, CancellationToken.None);

            moqSerializerManager
                .Verify(x => x.GetSerializer(typeof(FakeMessage)), Times.Once);

            mockProducer.Verify(x => x.ProduceAsync(It.IsAny<string>(),
                It.Is<Message<string, byte[]>>(i => i.Key == expectedMessageKey),
                It.IsAny<CancellationToken>()));
        }

        [Fact]
        public async Task ProduceShouldProduceMessageWithSerialisedMessage()
        {
            var stubMessageProducerBuilder = new Mock<IKafkaProducerBuilder>();
            var mockProducer = new Mock<IProducer<string, byte[]>>();
            stubMessageProducerBuilder
                .Setup(x => x.Build())
                .Returns(mockProducer.Object);
            var fakeMessage = new FakeMessage("some-key-id", "some-property-value");

            var moqSerializerManager = new Mock<IMessageSerializerManager>();
            moqSerializerManager
                .Setup(x => x.GetSerializer(typeof(FakeMessage)))
                .Returns(JsonMessageSerializer.Instance);

            var sut = new KafkaMessageProducer(stubMessageProducerBuilder.Object, moqSerializerManager.Object);
            await sut.ProduceAsync(fakeMessage.Key, fakeMessage, CancellationToken.None);

            moqSerializerManager
                .Verify(x => x.GetSerializer(typeof(FakeMessage)), Times.Once);

            mockProducer.Verify(x => x.ProduceAsync(It.IsAny<string>(),
                It.Is<Message<string, byte[]>>(i =>
                     Encoding.UTF8.GetString(i.Value) == JsonConvert.SerializeObject(fakeMessage)),
                It.IsAny<CancellationToken>()));
        }

        [Fact]
        public async Task ProduceShouldProduceMessageTypeAsHeader()
        {
            var stubMessageProducerBuilder = new Mock<IKafkaProducerBuilder>();
            var mockProducer = new Mock<IProducer<string, byte[]>>();
            stubMessageProducerBuilder
                .Setup(x => x.Build())
                .Returns(mockProducer.Object);
            var expectedMessageType = typeof(FakeMessage).AssemblyQualifiedName;
            var fakeMessage = new FakeMessage("some-key-id", "some-property-value");

            var moqSerializerManager = new Mock<IMessageSerializerManager>();
            moqSerializerManager
                .Setup(x => x.GetSerializer(typeof(FakeMessage)))
                .Returns(JsonMessageSerializer.Instance);

            var sut = new KafkaMessageProducer(stubMessageProducerBuilder.Object, moqSerializerManager.Object);
            await sut.ProduceAsync(fakeMessage.Key, fakeMessage, CancellationToken.None);

            moqSerializerManager
                .Verify(x => x.GetSerializer(typeof(FakeMessage)), Times.Once);

            mockProducer.Verify(x => x.ProduceAsync(It.IsAny<string>(),
                It.Is<Message<string, byte[]>>(i =>
                    Encoding.UTF8.GetString(i.Headers.GetLastBytes("message-type")) == expectedMessageType),
                It.IsAny<CancellationToken>()));
        }

        [Fact]
        public async Task ProduceShouldUseASingleProducerForMultipleRequests()
        {
            var mockMessageProducerBuilder = new Mock<IKafkaProducerBuilder>();
            var stubProducer = new Mock<IProducer<string, byte[]>>();
            mockMessageProducerBuilder
                .Setup(x => x.Build())
                .Returns(stubProducer.Object);
            var fakeMessage = new FakeMessage("some-key-id", "some-property-value");

            var moqSerializerManager = new Mock<IMessageSerializerManager>();
            moqSerializerManager
                .Setup(x => x.GetSerializer(typeof(FakeMessage)))
                .Returns(JsonMessageSerializer.Instance);

            var sut = new KafkaMessageProducer(mockMessageProducerBuilder.Object, moqSerializerManager.Object);
            await sut.ProduceAsync(fakeMessage.Key, fakeMessage, CancellationToken.None);
            await sut.ProduceAsync(fakeMessage.Key, fakeMessage, CancellationToken.None);
            await sut.ProduceAsync(fakeMessage.Key, fakeMessage, CancellationToken.None);

            moqSerializerManager
                .Verify(x => x.GetSerializer(typeof(FakeMessage)), Times.Exactly(3));

            mockMessageProducerBuilder.Verify(x => x.Build(), Times.Once);
        }

        [Fact]
        public async Task DisposeShouldDisposeProducerIfProduceHasBeenCalled()
        {
            var stubMessageProducerBuilder = new Mock<IKafkaProducerBuilder>();
            var mockProducer = new Mock<IProducer<string, byte[]>>();
            stubMessageProducerBuilder
                .Setup(x => x.Build())
                .Returns(mockProducer.Object);
            var fakeMessage = new FakeMessage("some-key-id", "some-property-value");

            var moqSerializerManager = new Mock<IMessageSerializerManager>();
            moqSerializerManager
                .Setup(x => x.GetSerializer(typeof(FakeMessage)))
                .Returns(JsonMessageSerializer.Instance);

            var sut = new KafkaMessageProducer(stubMessageProducerBuilder.Object, moqSerializerManager.Object);
            await sut.ProduceAsync(fakeMessage.Key, fakeMessage, CancellationToken.None);
            sut.Dispose();

            mockProducer.Verify(x => x.Dispose());
        }

        [Fact]
        public void DisposeShouldNotDisposeProducerIfProduceHasNotBeenCalled()
        {
            var stubMessageProducerBuilder = new Mock<IKafkaProducerBuilder>();
            var mockProducer = new Mock<IProducer<string, byte[]>>();
            stubMessageProducerBuilder
                .Setup(x => x.Build())
                .Returns(mockProducer.Object);

            var moqSerializerManager = new Mock<IMessageSerializerManager>();
            moqSerializerManager
                .Setup(x => x.GetSerializer(typeof(FakeMessage)))
                .Returns(JsonMessageSerializer.Instance);

            var sut = new KafkaMessageProducer(stubMessageProducerBuilder.Object, moqSerializerManager.Object);
            sut.Dispose();

            moqSerializerManager
                .Verify(x => x.GetSerializer(typeof(FakeMessage)), Times.Never);

            mockProducer.Verify(x => x.Dispose(), Times.Never);
        }
    }
}
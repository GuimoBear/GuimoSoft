using Confluent.Kafka;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using GuimoSoft.Bus.Abstractions;
using GuimoSoft.Bus.Core.Interfaces;
using GuimoSoft.Bus.Kafka.Producer;
using GuimoSoft.Bus.Tests.Fakes;
using GuimoSoft.Core.Serialization;
using Xunit;

namespace GuimoSoft.Bus.Tests.Producer
{
    public class KafkaEventProducerTests
    {
        [Fact]
        public void ConstructorShouldThrowIfAnyParameterIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new KafkaEventProducer(null, null));
            Assert.Throws<ArgumentNullException>(() => new KafkaEventProducer(Mock.Of<IKafkaProducerBuilder>(), null));
            Assert.Throws<ArgumentNullException>(() => new KafkaEventProducer(null, Mock.Of<IBusSerializerManager>()));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("  ")]
        public async Task ProduceAsyncShouldBeThrowArgumentExceptionIfKeyIsNullEmptyOrWriteSpace(string key)
        {
            var sut = new KafkaEventProducer(Mock.Of<IKafkaProducerBuilder>(), Mock.Of<IBusSerializerManager>());
            await Assert.ThrowsAsync<ArgumentException>(() => sut.Dispatch(key, new FakeEvent("", ""), ServerName.Default, FakeEvent.TOPIC_NAME, CancellationToken.None));
        }

        [Fact]
        public async Task ProduceAsyncShouldBeThrowArgumentNullExceptionIfEventIsNull()
        {
            var sut = new KafkaEventProducer(Mock.Of<IKafkaProducerBuilder>(), Mock.Of<IBusSerializerManager>());
            await Assert.ThrowsAsync<ArgumentNullException>(() => sut.Dispatch<FakeEvent>(Guid.NewGuid().ToString(), null, ServerName.Default, FakeEvent.TOPIC_NAME, CancellationToken.None));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("  ")]
        public async Task ProduceAsyncShouldBeThrowArgumentExceptionIfEndpointIsNullEmptyOrWriteSpace(string endpoint)
        {
            var sut = new KafkaEventProducer(Mock.Of<IKafkaProducerBuilder>(), Mock.Of<IBusSerializerManager>());
            await Assert.ThrowsAsync<ArgumentException>(() => sut.Dispatch(Guid.NewGuid().ToString(), new FakeEvent("", ""), ServerName.Default, endpoint, CancellationToken.None));
        }

        [Fact]
        public async Task ProduceAsyncShouldSerializeAndSendEventToKafka()
        {
            var moqEventProducerBuilder = new Mock<IKafkaProducerBuilder>();
            var mockProducer = new Mock<IProducer<string, byte[]>>();
            moqEventProducerBuilder
                .Setup(x => x.Build(It.IsAny<ServerName>()))
                .Returns(mockProducer.Object);

            var moqSerializerManager = new Mock<IBusSerializerManager>();
            moqSerializerManager
                .Setup(x => x.GetSerializer(BusName.Kafka, Finality.Produce, ServerName.Default, typeof(FakeEvent)))
                .Returns(JsonEventSerializer.Instance);

            var sut = new KafkaEventProducer(moqEventProducerBuilder.Object, moqSerializerManager.Object);

            await sut.Dispatch(Guid.NewGuid().ToString(), new FakeEvent("", ""), ServerName.Default, FakeEvent.TOPIC_NAME);

            moqSerializerManager
                .Verify(x => x.GetSerializer(BusName.Kafka, Finality.Produce, ServerName.Default, typeof(FakeEvent)), Times.Once);

            moqEventProducerBuilder
                .Verify(x => x.Build(ServerName.Default), Times.Once);

            mockProducer
                .Verify(x => x.ProduceAsync(It.IsAny<string>(), It.IsAny<Message<string, byte[]>>(), CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task DisposeShouldDisposeProducerIfProduceHasBeenCalled()
        {
            var stubEventProducerBuilder = new Mock<IKafkaProducerBuilder>();
            var mockProducer = new Mock<IProducer<string, byte[]>>();
            stubEventProducerBuilder
                .Setup(x => x.Build(It.IsAny<ServerName>()))
                .Returns(mockProducer.Object);
            var fakeEvent = new FakeEvent("some-key-id", "some-property-value");

            var moqSerializerManager = new Mock<IBusSerializerManager>();
            moqSerializerManager
                .Setup(x => x.GetSerializer(BusName.Kafka, Finality.Produce, ServerName.Default, typeof(FakeEvent)))
                .Returns(JsonEventSerializer.Instance);

            var sut = new KafkaEventProducer(stubEventProducerBuilder.Object, moqSerializerManager.Object);
            await sut.Dispatch(Guid.NewGuid().ToString(), new FakeEvent("", ""), ServerName.Default, FakeEvent.TOPIC_NAME);
            sut.Dispose();

            mockProducer.Verify(x => x.Dispose());
        }

        [Fact]
        public void DisposeShouldNotDisposeProducerIfProduceHasNotBeenCalled()
        {
            var stubEventProducerBuilder = new Mock<IKafkaProducerBuilder>();
            var mockProducer = new Mock<IProducer<string, byte[]>>();
            stubEventProducerBuilder
                .Setup(x => x.Build(It.IsAny<ServerName>()))
                .Returns(mockProducer.Object);

            var moqSerializerManager = new Mock<IBusSerializerManager>();
            moqSerializerManager
                .Setup(x => x.GetSerializer(BusName.Kafka, Finality.Produce, ServerName.Default, typeof(FakeEvent)))
                .Returns(JsonEventSerializer.Instance);

            var sut = new KafkaEventProducer(stubEventProducerBuilder.Object, moqSerializerManager.Object);
            sut.Dispose();

            moqSerializerManager
                .Verify(x => x.GetSerializer(BusName.Kafka, Finality.Produce, ServerName.Default, typeof(FakeEvent)), Times.Never);

            mockProducer.Verify(x => x.Dispose(), Times.Never);
        }
    }
}
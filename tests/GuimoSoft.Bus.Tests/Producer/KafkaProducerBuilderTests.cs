using Microsoft.Extensions.Options;
using System;
using GuimoSoft.Bus.Kafka.Common;
using GuimoSoft.Bus.Kafka.Producer;
using Xunit;

namespace GuimoSoft.Bus.Tests.Producer
{
    public class KafkaProducerBuilderTests
    {
        [Fact]
        public void ConstructorShouldCreateSampleProducerBuilder()
        {
            var kafkaOptions = Options.Create(new KafkaOptions());

            var sut = new KafkaProducerBuilder(kafkaOptions);

            Assert.IsType<KafkaProducerBuilder>(sut);
        }

        [Fact]
        public void ConstructorShouldThrowIfOptionsIsNull()
        {
            IOptions<KafkaOptions> kafkaOptions = null;

            Assert.Throws<ArgumentNullException>(() => new KafkaProducerBuilder(kafkaOptions));
        }

        [Fact]
        public void BuildShouldReturnNonNullProducer()
        {
            var kafkaOptions = Options.Create(new KafkaOptions());

            var sut = new KafkaProducerBuilder(kafkaOptions);

            var producer = sut.Build();

            Assert.NotNull(producer);
        }
    }
}
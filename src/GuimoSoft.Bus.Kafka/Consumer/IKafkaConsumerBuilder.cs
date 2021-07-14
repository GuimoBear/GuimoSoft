using Confluent.Kafka;

namespace GuimoSoft.Bus.Kafka.Consumer
{
    public interface IKafkaConsumerBuilder
    {
        IConsumer<string, byte[]> Build();
    }
}
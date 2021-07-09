using Confluent.Kafka;

namespace GuimoSoft.MessageBroker.Kafka.Consumer
{
    public interface IKafkaConsumerBuilder
    {
        IConsumer<string, string> Build();
    }
}
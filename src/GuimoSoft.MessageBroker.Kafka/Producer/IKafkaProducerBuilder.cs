using Confluent.Kafka;

namespace GuimoSoft.MessageBroker.Kafka.Producer
{
    public interface IKafkaProducerBuilder
    {
        IProducer<string, string> Build();
    }
}
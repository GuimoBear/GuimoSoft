using Confluent.Kafka;

namespace GuimoSoft.Bus.Kafka.Producer
{
    public interface IKafkaProducerBuilder
    {
        IProducer<string, byte[]> Build();
    }
}
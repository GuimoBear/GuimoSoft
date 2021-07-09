using System.Threading;

namespace GuimoSoft.MessageBroker.Kafka.Consumer
{
    public interface IKafkaTopicMessageConsumer
    {
        void ConsumeUntilCancellationIsRequested(string topic, CancellationToken cancellationToken);
    }
}
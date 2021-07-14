using System.Threading;

namespace GuimoSoft.Bus.Kafka.Consumer
{
    public interface IKafkaTopicMessageConsumer
    {
        void ConsumeUntilCancellationIsRequested(string topic, CancellationToken cancellationToken);
    }
}
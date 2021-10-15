using System.Threading;

namespace GuimoSoft.Bus.Kafka.Consumer
{
    public interface IKafkaEventConsumerManager
    {
        void StartConsumers(CancellationToken cancellationToken);
    }
}
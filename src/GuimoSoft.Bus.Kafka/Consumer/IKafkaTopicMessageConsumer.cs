using System;
using System.Threading;

namespace GuimoSoft.Bus.Kafka.Consumer
{
    public interface IKafkaTopicEventConsumer
    {
        void ConsumeUntilCancellationIsRequested(Enum @switch, string topic, CancellationToken cancellationToken);
    }
}
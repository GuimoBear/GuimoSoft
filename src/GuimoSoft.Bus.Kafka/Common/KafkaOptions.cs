using Confluent.Kafka;

namespace GuimoSoft.Bus.Kafka.Common
{
    public class KafkaOptions
    {
        public string KafkaBootstrapServers { get; set; }
        public string ConsumerGroupId { get; set; }
        public AutoOffsetReset AutoOffsetReset { get; set; } = AutoOffsetReset.Earliest;
        public Acks Acks { get; set; } = Acks.None;
    }
}
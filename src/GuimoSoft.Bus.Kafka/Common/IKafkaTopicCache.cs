using System;
using System.Collections.Generic;
using GuimoSoft.Bus.Abstractions;

namespace GuimoSoft.Bus.Kafka.Common
{
    public interface IKafkaTopicCache
    {
        string this[IMessage message] { get; }
        string this[Type type] { get; }
        IReadOnlyCollection<Type> this[string topic] { get; }
    }
}

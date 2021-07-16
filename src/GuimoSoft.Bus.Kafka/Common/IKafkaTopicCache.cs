using GuimoSoft.Bus.Abstractions;
using System;
using System.Collections.Generic;

namespace GuimoSoft.Bus.Kafka.Common
{
    public interface IKafkaTopicCache
    {
        string this[IMessage message] { get; }
        string this[Type type] { get; }
        IReadOnlyCollection<Type> this[string topic] { get; }
    }
}

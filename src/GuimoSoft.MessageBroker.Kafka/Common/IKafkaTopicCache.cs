using System;
using System.Collections.Generic;
using GuimoSoft.MessageBroker.Abstractions;

namespace GuimoSoft.MessageBroker.Kafka.Common
{
    public interface IKafkaTopicCache
    {
        string this[IMessage message] { get; }
        string this[Type type] { get; }
        IReadOnlyCollection<Type> this[string topic] { get; }
    }
}

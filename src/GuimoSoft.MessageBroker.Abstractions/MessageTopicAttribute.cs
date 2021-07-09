using System;

namespace GuimoSoft.MessageBroker.Abstractions
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class MessageTopicAttribute : Attribute
    {
        public MessageTopicAttribute(string topic, bool isEnvironmentVariableName = false, string defaultTopicName = default)
        {
            if (!isEnvironmentVariableName)
            {
                Topic = topic;
                return;
            }
            Topic = Environment.GetEnvironmentVariable(topic);
            if (string.IsNullOrEmpty(Topic))
                Topic = defaultTopicName;
        }

        public string Topic { get; }
    }
}
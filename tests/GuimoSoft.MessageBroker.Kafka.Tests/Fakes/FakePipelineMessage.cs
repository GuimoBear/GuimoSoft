using System.Collections.Generic;
using GuimoSoft.MessageBroker.Abstractions;

namespace GuimoSoft.MessageBroker.Kafka.Tests.Fakes
{
    public class FakePipelineMessage : IMessage
    {
        public string LastMiddlewareToRun { get; }
        public List<string> MiddlewareNames { get; } = new List<string>();

        public FakePipelineMessage()
        {

        }

        public FakePipelineMessage(string lastMiddlewareToRun)
        {
            LastMiddlewareToRun = lastMiddlewareToRun;
        }
    }
}

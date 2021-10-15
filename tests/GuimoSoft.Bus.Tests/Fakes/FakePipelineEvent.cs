using System.Collections.Generic;
using GuimoSoft.Bus.Abstractions;

namespace GuimoSoft.Bus.Tests.Fakes
{
    public class FakePipelineEvent : IEvent
    {
        public const string TOPIC_NAME = "fake-pipeline-topic";

        public string LastMiddlewareToRun { get; }
        public List<string> MiddlewareNames { get; } = new List<string>();

        public FakePipelineEvent()
        {

        }

        public FakePipelineEvent(string lastMiddlewareToRun)
        {
            LastMiddlewareToRun = lastMiddlewareToRun;
        }
    }
}

using System;
using System.Text.Json.Serialization;
using GuimoSoft.Bus.Abstractions;

namespace GuimoSoft.Benchmark.Bus.Handlers.Benchmark
{
    public class BenchmarkEvent : IEvent
    {
        public const string TOPIC_NAME = "benchmark-event-topic";

        [JsonPropertyName(nameof(Id))]
        public Guid Id { get; private set; }

        [JsonConstructor]
        public BenchmarkEvent(Guid id)
        {
            Id = id;
        }
    }
}

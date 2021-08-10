using System;
using System.Text.Json.Serialization;
using GuimoSoft.Bus.Abstractions;

namespace GuimoSoft.Benchmark.Bus.Handlers.Benchmark
{
    public class BenchmarkMessage : IMessage
    {
        public const string TOPIC_NAME = "benchmark-message-topic";

        [JsonPropertyName(nameof(Id))]
        public Guid Id { get; private set; }

        [JsonConstructor]
        public BenchmarkMessage(Guid id)
        {
            Id = id;
        }
    }
}

using FluentAssertions;
using GuimoSoft.Bus.Abstractions;
using GuimoSoft.Bus.Core.Internal;
using GuimoSoft.Bus.Tests.Fakes;
using GuimoSoft.Core.Serialization;
using Xunit;

namespace GuimoSoft.Bus.Tests.Core.Internal
{
    public class BusSerializerManagerTests
    {
        [Fact]
        public void BusSerializerManagerFacts()
        {
            lock (Utils.Lock)
            {
                var sut = new BusSerializerManager();

                sut.GetSerializer(BusName.Kafka, Finality.Consume, ServerName.Default, typeof(FakePipelineEvent))
                    .Should().BeSameAs(EventSerializerManager.Instance.GetSerializer(typeof(FakePipelineEvent)));

                sut.AddTypedSerializer(BusName.Kafka, Finality.Consume, ServerName.Default, FakePipelineEventSerializer.Instance);

                sut.GetSerializer(BusName.Kafka, Finality.Consume, ServerName.Default, typeof(FakePipelineEvent))
                    .Should().BeSameAs(FakePipelineEventSerializer.Instance);

                Utils.ResetarEventSerializerManager();
            }
        }
    }
}

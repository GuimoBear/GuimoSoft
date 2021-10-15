using FluentAssertions;
using System;
using GuimoSoft.Bus.Tests.Fakes;
using GuimoSoft.Core.Serialization;
using Xunit;

namespace GuimoSoft.Bus.Tests.Core
{
    public class EventSerializerManagerTests
    {
        [Fact]
        public void When_SetDefaultSerializerWithNullSerializer_Then_ThrowArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => EventSerializerManager.Instance.SetDefaultSerializer(null));
        }

        [Fact]
        public void When_SetDefaultSerializerWithAnValidSerializer_Then_SetSerializer()
        {
            lock (Utils.Lock)
            { 
                EventSerializerManager.Instance.GetSerializer(typeof(FakeEvent))
                    .Should().BeSameAs(JsonEventSerializer.Instance);

                EventSerializerManager.Instance.GetSerializer(typeof(OtherFakeEvent))
                    .Should().BeSameAs(JsonEventSerializer.Instance);

                EventSerializerManager.Instance.SetDefaultSerializer(FakeDefaultSerializer.Instance);

                EventSerializerManager.Instance.GetSerializer(typeof(FakeEvent))
                    .Should().BeSameAs(FakeDefaultSerializer.Instance);

                EventSerializerManager.Instance.GetSerializer(typeof(FakeEvent))
                    .Should().BeSameAs(FakeDefaultSerializer.Instance);

                EventSerializerManager.Instance.SetDefaultSerializer(JsonEventSerializer.Instance);

                EventSerializerManager.Instance.GetSerializer(typeof(FakeEvent))
                    .Should().BeSameAs(JsonEventSerializer.Instance);

                EventSerializerManager.Instance.GetSerializer(typeof(OtherFakeEvent))
                    .Should().BeSameAs(JsonEventSerializer.Instance);

                Utils.ResetarEventSerializerManager();
            }
        }

        [Fact]
        public void When_AddTypedSerializerWithNullSerializer_Then_ThrowArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => EventSerializerManager.Instance.AddTypedSerializer<FakeEvent>(null));
        }

        [Fact]
        public void When_AddTypedSerializerWithAnValidTypedSerializer_Then_SetTypedSerializer()
        {
            lock (Utils.Lock)
            {
                EventSerializerManager.Instance.GetSerializer(typeof(OtherFakeEvent))
                    .Should().BeSameAs(JsonEventSerializer.Instance);

               EventSerializerManager.Instance.AddTypedSerializer(OtherFakeEventSerializer.Instance);

               EventSerializerManager.Instance.GetSerializer(typeof(OtherFakeEvent))
                    .Should().BeSameAs(OtherFakeEventSerializer.Instance);

                Utils.ResetarEventSerializerManager();
            }
        }
    }
}

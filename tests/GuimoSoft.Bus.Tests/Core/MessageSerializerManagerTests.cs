using FluentAssertions;
using System;
using GuimoSoft.Bus.Tests.Fakes;
using GuimoSoft.Serialization;
using Xunit;

namespace GuimoSoft.Bus.Tests.Core
{
    public class MessageSerializerManagerTests
    {
        [Fact]
        public void When_SetDefaultSerializerWithNullSerializer_Then_ThrowArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => MessageSerializerManager.Instance.SetDefaultSerializer(null));
        }

        [Fact]
        public void When_SetDefaultSerializerWithAnValidSerializer_Then_SetSerializer()
        {
            MessageSerializerManager.Instance.SetDefaultSerializer(FakeDefaultSerializer.Instance);
            MessageSerializerManager.Instance.SetDefaultSerializer(JsonMessageSerializer.Instance);
        }

        [Fact]
        public void When_AddTypedSerializerWithNullSerializer_Then_ThrowArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => MessageSerializerManager.Instance.AddTypedSerializer<OtherFakeMessage>(null));
        }

        [Fact]
        public void When_AddTypedSerializerWithAnValidTypedSerializer_Then_SetTypedSerializer()
        {
            MessageSerializerManager.Instance.AddTypedSerializer(OtherFakeMessageSerializer.Instance);
            var actual = MessageSerializerManager.Instance.GetSerializer(typeof(OtherFakeMessage));

            ReferenceEquals(OtherFakeMessageSerializer.Instance, actual)
                .Should().BeTrue();
        }
    }
}

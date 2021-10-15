using FluentAssertions;
using System.Text.Json;
using GuimoSoft.Bus.Tests.Fakes;
using Xunit;

namespace GuimoSoft.Bus.Tests
{
    public class TypedSerializerTests
    {
        [Fact]
        public void When_Serialize_Then_NotThrowAnyException()
        {
            var fakeEvent = new OtherFakeEvent("test", "115");

            var expected = JsonSerializer.SerializeToUtf8Bytes(fakeEvent);

            var actual = OtherFakeEventSerializer.Instance.Serialize(fakeEvent);

            actual
                .Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void When_Deserialize_Then_NotThrowAnyException()
        {
            var expected = new OtherFakeEvent("test", "115");

            var serializedContent = JsonSerializer.SerializeToUtf8Bytes(expected);

            var actual = OtherFakeEventSerializer.Instance.Deserialize(typeof(OtherFakeEvent), serializedContent);

            actual
                .Should().BeEquivalentTo(expected);
        }
    }
}

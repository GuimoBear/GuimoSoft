using System;
using GuimoSoft.Bus.Abstractions;
using Xunit;

namespace GuimoSoft.Bus.Tests.Common
{
    public class MessageTopicAttributeTests
    {
        [Fact]
        public void ConstructorReturnsMessageTopicAttribute()
        {
            const string topic = "sample-topic";

            var sut = new MessageTopicAttribute(topic);

            Assert.Equal(topic, sut.Topic);
        }

        [Fact]
        public void ConstructorUsingEnvironmentVariableReturnsMessageTopicAttribute()
        {
            const string topic = "topic-123";
            try
            {
                Environment.SetEnvironmentVariable("test-topic-name", topic);

                var sut = new MessageTopicAttribute("test-topic-name", true);

                Assert.Equal(topic, sut.Topic);
            }
            finally
            {
                Environment.SetEnvironmentVariable("test-topic-name", null);
            }
        }

        [Fact]
        public void ConstructorUsingNotFoundEnvironmentVariableAndDefaultValueReturnsMessageTopicAttribute()
        {
            const string topic = "topic-1234";
            var sut = new MessageTopicAttribute("test-topic-name-2", true, topic);

            Assert.Equal(topic, sut.Topic);
        }
    }
}
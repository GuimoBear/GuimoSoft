using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using GuimoSoft.Bus.Abstractions;
using GuimoSoft.Bus.Core.Internal;
using GuimoSoft.Bus.Tests.Fakes;
using Xunit;

namespace GuimoSoft.Bus.Tests.Core.Internal
{
    public class eventTypeCacheTests
    {
        [Fact]
        public void AddShouldBeThrowArgumentExceptionIfEventNotImplementIEvent()
        {
            var sut = new EventTypeCache();
            Assert.Throws<ArgumentException>(() => sut.Add(BusName.Kafka, Finality.Produce, ServerName.Default, typeof(eventTypeCacheTests), "test"));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("  ")]
        public void AddShouldBeThrowArgumentExceptionIfEndpointIsNullEmptyOrWritespace(string endpoint)
        {
            var sut = new EventTypeCache();
            Assert.Throws<ArgumentException>(() => sut.Add(BusName.Kafka, Finality.Produce, ServerName.Default, typeof(FakeEvent), endpoint));
        }

        [Fact]
        public void AddWithNullSwitchFacts()
        {
            var sut = new EventTypeCache();
            Assert.Throws<ArgumentNullException>(() => sut.Add(BusName.Kafka, Finality.Produce, null, typeof(FakeEvent), "test"));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("  ")]
        public void AddWithInvalidEndpointFacts(string endpoint)
        {
            var sut = new EventTypeCache();
            Assert.Throws<ArgumentException>(() => sut.Add(BusName.Kafka, Finality.Produce, ServerName.Default, typeof(FakeEvent), endpoint));
        }

        [Fact]
        public void AddWithSameEndpointFacts()
        {
            var sut = new EventTypeCache();

            sut.Add(BusName.Kafka, Finality.Produce, ServerName.Default, typeof(FakeEvent), "test");

            sut.Add(BusName.Kafka, Finality.Produce, ServerName.Default, typeof(FakeEvent), "test");

            sut.Get(typeof(FakeEvent))
                .ToList().Should().HaveCount(1);
        }

        [Fact]
        public void AddTwoDiferentEndpointsFacts()
        {
            var sut = new EventTypeCache();

            sut.Add(BusName.Kafka, Finality.Produce, ServerName.Default, typeof(FakeEvent), "test");

            sut.Add(BusName.Kafka, Finality.Produce, ServerName.Default, typeof(FakeEvent), "test 2");

            sut.Get(typeof(FakeEvent))
                .ToList().Should().HaveCount(2);
        }

        [Fact]
        public void GetSwitchersWithNonExistingDataFacts()
        {
            var sut = new EventTypeCache();

            sut.Add(BusName.Kafka, Finality.Produce, ServerName.Default, typeof(FakeEvent), "test");

            Assert.Throws<InvalidOperationException>(() => sut.GetSwitchers(BusName.None, Finality.Produce));
            Assert.Throws<InvalidOperationException>(() => sut.GetSwitchers(BusName.Kafka, Finality.Consume));
        }

        [Fact]
        public void GetSwitchersWithExistingDataFacts()
        {
            var sut = new EventTypeCache();

            sut.Add(BusName.Kafka, Finality.Produce, ServerName.Default, typeof(FakeEvent), "test");

            sut.GetSwitchers(BusName.Kafka, Finality.Produce)
                .ToList().Should().NotBeEmpty();
        }

        [Fact]
        public void GetEndpointsWithNullSwitchFacts()
        {
            var sut = new EventTypeCache();
            Assert.Throws<ArgumentNullException>(() => sut.GetEndpoints(BusName.Kafka, Finality.Produce, null));
        }

        [Fact]
        public void GetEndpointsWithNonExistingDataFacts()
        {
            var sut = new EventTypeCache();
            Assert.Throws<ArgumentNullException>(() => sut.GetEndpoints(BusName.Kafka, Finality.Produce, null));

            Assert.Throws<KeyNotFoundException>(() => sut.GetEndpoints(BusName.Kafka, Finality.Produce, ServerName.Default));

            sut.Add(BusName.Kafka, Finality.Produce, ServerName.Default, typeof(FakeEvent), "test");

            Assert.Throws<KeyNotFoundException>(() => sut.GetEndpoints(BusName.Kafka, Finality.Consume, ServerName.Default));

            Assert.Throws<KeyNotFoundException>(() => sut.GetEndpoints(BusName.Kafka, Finality.Produce, FakeServerName.FakeHost1));
        }

        [Fact]
        public void GetEndpointsWithExistingDataFacts()
        {
            var sut = new EventTypeCache();

            sut.Add(BusName.Kafka, Finality.Produce, ServerName.Default, typeof(FakeEvent), "test");

            sut.GetEndpoints(BusName.Kafka, Finality.Produce, ServerName.Default)
                .ToList().Should().NotBeEmpty();
        }

        [Fact]
        public void GetByEndpointWithNullSwitchFacts()
        {
            var sut = new EventTypeCache();

            Assert.Throws<ArgumentNullException>(() => sut.Get(BusName.Kafka, Finality.Produce, null, "test"));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void GetByEndpointWithInvalidEndpointFacts(string endpoint)
        {
            var sut = new EventTypeCache();

            Assert.Throws<ArgumentException>(() => sut.Get(BusName.Kafka, Finality.Produce, ServerName.Default, endpoint));
        }

        [Fact]
        public void GetByEndpointWithNonExistingDataFacts()
        {
            var sut = new EventTypeCache();

            Assert.Throws<KeyNotFoundException>(() => sut.Get(BusName.Kafka, Finality.Produce, FakeServerName.FakeHost1, FakeEvent.TOPIC_NAME)); 
            
            sut.Add(BusName.Kafka, Finality.Produce, FakeServerName.FakeHost1, typeof(AnotherFakeEvent), "test");

            Assert.Throws<KeyNotFoundException>(() => sut.Get(BusName.Kafka, Finality.Produce, FakeServerName.FakeHost2, FakeEvent.TOPIC_NAME));

            sut.Add(BusName.Kafka, Finality.Produce, FakeServerName.FakeHost2, typeof(FakeEvent), "test");

            Assert.Throws<KeyNotFoundException>(() => sut.Get(BusName.Kafka, Finality.Produce, FakeServerName.FakeHost1, FakeEvent.TOPIC_NAME));
        }

        [Fact]
        public void GetByEndpointWithExistingDataFacts()
        {
            var sut = new EventTypeCache();

            sut.Add(BusName.Kafka, Finality.Produce, FakeServerName.FakeHost1, typeof(FakeEvent), FakeEvent.TOPIC_NAME);

            sut.Get(BusName.Kafka, Finality.Produce, FakeServerName.FakeHost1, FakeEvent.TOPIC_NAME)
                .ToList().Should().NotBeEmpty();
        }

        [Fact]
        public void GetByeventTypeNullFacts()
        {
            var sut = new EventTypeCache();

            Assert.Throws<ArgumentNullException>(() => sut.Get(BusName.Kafka, Finality.Produce, FakeServerName.FakeHost1, null as Type));
        }

        [Fact]
        public void GetByEventInstanceWithNonExistingData()
        {
            var fakeEvent = new FakeEvent("test", "test");

            var sut = new EventTypeCache();

            Assert.Throws<ArgumentNullException>(() => sut.Get(BusName.Kafka, Finality.Produce, null, fakeEvent));

            Assert.Throws<ArgumentNullException>(() => sut.Get(BusName.Kafka, Finality.Produce, FakeServerName.FakeHost1, null as FakeEvent));

            sut.Add(BusName.Kafka, Finality.Consume, FakeServerName.FakeHost1, typeof(FakeEvent), "test");

            Assert.Throws<KeyNotFoundException>(() => sut.Get(BusName.Kafka, Finality.Produce, FakeServerName.FakeHost1, fakeEvent));

            sut.Add(BusName.Kafka, Finality.Produce, FakeServerName.FakeHost1, typeof(FakeEvent), "test");

            Assert.Throws<KeyNotFoundException>(() => sut.Get(BusName.Kafka, Finality.Produce, FakeServerName.FakeHost2, fakeEvent));

            sut.Add(BusName.Kafka, Finality.Produce, FakeServerName.FakeHost2, typeof(AnotherFakeEvent), "test");

            Assert.Throws<KeyNotFoundException>(() => sut.Get(BusName.Kafka, Finality.Produce, FakeServerName.FakeHost2, fakeEvent));
        }

        [Fact]
        public void GetByEventInstanceFacts()
        {
            var fakeEvent = new FakeEvent("test", "test");

            var sut = new EventTypeCache();

            sut.Add(BusName.Kafka, Finality.Produce, FakeServerName.FakeHost2, typeof(FakeEvent), "test");

            sut.Get(BusName.Kafka, Finality.Produce, FakeServerName.FakeHost2, fakeEvent)
                .ToList().Should().NotBeEmpty();
        }

        [Fact]
        public void GetByeventTypeWithNullTypeFacts()
        {
            var sut = new EventTypeCache();

            Assert.Throws<ArgumentNullException>(() => sut.Get(null));
        }

        [Fact]
        public void GetByeventTypeWithNonExistingDataFacts()
        {
            var sut = new EventTypeCache();

            Assert.Throws<KeyNotFoundException>(() => sut.Get(typeof(FakeEvent)));

            sut.Add(BusName.Kafka, Finality.Consume, FakeServerName.FakeHost1, typeof(FakeEvent), "test");

            Assert.Throws<KeyNotFoundException>(() => sut.Get(typeof(FakeEvent)));
        }

        [Fact]
        public void GetByeventTypeWithExistingDataFacts()
        {
            var sut = new EventTypeCache();

            sut.Add(BusName.Kafka, Finality.Produce, FakeServerName.FakeHost1, typeof(FakeEvent), "test");

            sut.Get(typeof(FakeEvent))
                .Should().HaveCount(1);

            sut.Add(BusName.Kafka, Finality.Produce, FakeServerName.FakeHost2, typeof(FakeEvent), "test");

            sut.Get(typeof(FakeEvent))
                .Should().HaveCount(2);

            sut.Add(BusName.Kafka, Finality.Produce, FakeServerName.FakeHost2, typeof(FakeEvent), "test");

            sut.Get(typeof(FakeEvent))
                .ToList().Should().HaveCount(2);
        }

        [Fact]
        public void eventTypeItemEqualFacts()
        {
            new EventTypeCache.EventTypeItem(BusName.Kafka, Finality.Produce, FakeServerName.FakeHost2, typeof(FakeEvent), "test")
                .Equals(null)
                .Should().BeFalse();
        }
        /*
        [Fact]
        public void GetByeventTypeFacts()
        {
            var sut = new eventTypeCache();

            Assert.Throws<ArgumentNullException>(() => sut.Get(null));

            Assert.Throws<KeyNotFoundException>(() => sut.Get(typeof(FakeEvent)));

            sut.Add(BusName.Kafka, Finality.Consume, FakeServerName.FakeHost1, typeof(FakeEvent), "test");

            Assert.Throws<KeyNotFoundException>(() => sut.Get(typeof(FakeEvent)));

            sut.Add(BusName.Kafka, Finality.Produce, FakeServerName.FakeHost1, typeof(FakeEvent), "test");

            sut.Get(typeof(FakeEvent))
                .Should().HaveCount(1);

            sut.Add(BusName.Kafka, Finality.Produce, FakeServerName.FakeHost2, typeof(FakeEvent), "test");

            sut.Get(typeof(FakeEvent))
                .Should().HaveCount(2);

            sut.Add(BusName.Kafka, Finality.Produce, FakeServerName.FakeHost2, typeof(FakeEvent), "test");

            sut.Get(typeof(FakeEvent))
                .ToList().Should().HaveCount(2);

            new eventTypeCache.eventTypeItem(BusName.Kafka, Finality.Produce, FakeServerName.FakeHost2, typeof(FakeEvent), "test")
                .Equals(null)
                .Should().BeFalse();
        }
        */
    }
}

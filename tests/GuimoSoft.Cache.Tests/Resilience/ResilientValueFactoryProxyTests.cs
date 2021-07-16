using FluentAssertions;
using GuimoSoft.Cache.Resilience;
using GuimoSoft.Cache.Tests.Fakes;
using Moq;
using Polly;
using System;
using System.Threading.Tasks;
using Xunit;

namespace GuimoSoft.Cache.Tests.Resilience
{
    public class ResilientValueFactoryProxyTests
    {
        [Fact]
        public void When_ConstructWithNullPolicy_Then_ThrowArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ResilientValueFactoryProxy<FakeValue>(null));
        }

        [Fact]
        public void When_Produce_Then_ReturnInstance()
        {
            var value = new FakeValue("test", 5);

            var moqPolicy = new Mock<IAsyncPolicy<FakeValue>>();
            moqPolicy
                .Setup(x => x.ExecuteAsync(It.IsAny<Func<Task<FakeValue>>>()))
                .ReturnsAsync(value);

            var sut = new ResilientValueFactoryProxy<FakeValue>(moqPolicy.Object);

            var createdValue = sut.Produce(() => new FakeValue("test", 5));

            ReferenceEquals(createdValue, value)
                .Should().BeTrue();

            moqPolicy.Verify(x => x.ExecuteAsync(It.IsAny<Func<Task<FakeValue>>>()), Times.Once);
        }

        [Fact]
        public async Task When_ProduceAsync_Then_ReturnInstance()
        {
            var value = new FakeValue("test", 5);

            var moqPolicy = new Mock<IAsyncPolicy<FakeValue>>();
            moqPolicy
                .Setup(x => x.ExecuteAsync(It.IsAny<Func<Task<FakeValue>>>()))
                .ReturnsAsync(value);

            var sut = new ResilientValueFactoryProxy<FakeValue>(moqPolicy.Object);

            var createdValue = await sut.ProduceAsync(() => Task.FromResult(new FakeValue("test", 5)));

            ReferenceEquals(createdValue, value)
                .Should().BeTrue();

            moqPolicy.Verify(x => x.ExecuteAsync(It.IsAny<Func<Task<FakeValue>>>()), Times.Once);
        }
    }
}

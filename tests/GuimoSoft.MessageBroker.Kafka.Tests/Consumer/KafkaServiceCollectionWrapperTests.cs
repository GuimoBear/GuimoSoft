using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using GuimoSoft.MessageBroker.Kafka.Consumer;
using GuimoSoft.MessageBroker.Kafka.Tests.Fakes;
using Xunit;

namespace GuimoSoft.MessageBroker.Kafka.Tests.Consumer
{
    public class KafkaServiceCollectionWrapperTests
    {
        private const int SERVICE_COLLECTION_MIN_LENGTH = 3;

        [Fact]
        public void Se_NaoExistemServicosRegistrados_Entao_CountRetornaMinimo()
        {
            var sut = new KafkaServiceCollectionWrapper(new ServiceCollection());

            sut.Count
                .Should().Be(SERVICE_COLLECTION_MIN_LENGTH);
        }

        [Fact]
        public void Se_NaoExistemServicosRegistrados_Entao_ExistemApenasAsInstanciasCriadadPorPadrao()
        {
            var moqServiceCollection = new Mock<IServiceCollection>();

            moqServiceCollection
                .Setup(x => x.Add(It.Is<ServiceDescriptor>(sd => sd.ServiceType.Equals(typeof(IMessageMiddlereManager)))));

            moqServiceCollection
                .Setup(x => x.Add(It.IsAny<ServiceDescriptor>()))
                .Verifiable();

            var sut = new KafkaServiceCollectionWrapper(moqServiceCollection.Object);

            moqServiceCollection
                .Verify(x => x.Add(It.Is<ServiceDescriptor>(sd => sd.ServiceType.Equals(typeof(IMessageMiddlereManager)))), Times.Once);

            moqServiceCollection
                .Verify(x => x.Add(It.Is<ServiceDescriptor>(sd => sd.ServiceType.Equals(typeof(IMessageMiddlereExecutorProvider)))), Times.Once);

            moqServiceCollection
                .Verify(x => x.Add(It.Is<ServiceDescriptor>(sd => sd.ServiceType.Equals(typeof(IMessageMiddlewareRegister)))), Times.Once);
        }

        [Fact]
        public void Se_IsReadoOllyExecutado_Entao_RetornaFalse()
        {
            var sut = new KafkaServiceCollectionWrapper(new ServiceCollection());

            sut.IsReadOnly
                .Should().BeFalse();
        }

        [Fact]
        public void Dado_UmServiceDescriptorDeFakeMessage_Se_AddExecutado_Entao_NaoEstouraUmaExcecao()
        {
            var expected = new ServiceDescriptor(typeof(FakeMessage), new FakeMessage("", ""));

            var sut = new KafkaServiceCollectionWrapper(new ServiceCollection());

            sut.Add(expected);

            sut.Count
                .Should().Be(SERVICE_COLLECTION_MIN_LENGTH + 1);

            sut[SERVICE_COLLECTION_MIN_LENGTH]
                .Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void Dado_UmServiceDescriptorDeFakeMessage_Se_AddExecutadoEValorSetadoParaOutro_Entao_NaoEstouraUmaExcecao()
        {
            var expected = new ServiceDescriptor(typeof(FakeMessage), new FakeMessage("", ""));
            var newExpected = new ServiceDescriptor(typeof(OtherFakeMessage), new OtherFakeMessage("", ""));

            var sut = new KafkaServiceCollectionWrapper(new ServiceCollection());

            sut.Add(expected);

            sut.Count
                .Should().Be(SERVICE_COLLECTION_MIN_LENGTH + 1);

            sut[SERVICE_COLLECTION_MIN_LENGTH]
                .Should().BeEquivalentTo(expected);

            sut[SERVICE_COLLECTION_MIN_LENGTH] = newExpected;

            sut[SERVICE_COLLECTION_MIN_LENGTH]
                .Should().BeEquivalentTo(newExpected);
        }

        [Fact]
        public void Dado_UmServiceCollectionComUmaFakeMessageRegistrada_Se_ClearExecutado_Entao_CountRetornaZero()
        {
            var sut = new KafkaServiceCollectionWrapper(new ServiceCollection().AddSingleton<FakeMessage>());

            sut.Count
                .Should().Be(SERVICE_COLLECTION_MIN_LENGTH + 1);

            sut.Clear();

            sut.Count
                .Should().Be(0);
        }

        [Fact]
        public void Dado_DoisMiddlewares_Se_WithMessageMiddlewareExecutado_Entao_InstanciasDosMiddlewaresSaoRegistradas()
        {
            var sut = new KafkaServiceCollectionWrapper(new ServiceCollection());

            sut.WithMessageMiddleware<FakeMessage, FakeMessageMiddleware>()
               .WithMessageMiddleware<FakeMessage, FakeMessageThrowExceptionMiddleware>(prov => new FakeMessageThrowExceptionMiddleware());

            sut.Count
                .Should().Be(SERVICE_COLLECTION_MIN_LENGTH + 2);

            using (var scope = sut.BuildServiceProvider())
            {
                scope.GetService<FakeMessageMiddleware>()
                    .Should().NotBeNull();

                scope.GetService<FakeMessageThrowExceptionMiddleware>()
                    .Should().NotBeNull();
            }
        }
    }
}

using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using GuimoSoft.Bus.Abstractions.Consumer;
using GuimoSoft.Bus.Kafka.Common;
using GuimoSoft.Bus.Tests.Fakes;
using Xunit;

namespace GuimoSoft.Bus.Tests.Common
{
    public class KafkaCacheTests
    {
        private IServiceCollection CreateServiceCollectionWithoutAnotherFakeMessageNotificationHandler()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddTransient(s => Mock.Of<INotificationHandler<MessageNotification<FakeMessage>>>());
            serviceCollection.AddTransient(s => Mock.Of<INotificationHandler<MessageNotification<OtherFakeMessage>>>());

            return serviceCollection;
        }

        [Fact]
        public void Dado_UmaFakeMessageNula_Se_TentarPegarOTopico_Entao_EstouraArgumentNullException()
        {
            var sut = new KafkaTopicCache(CreateServiceCollectionWithoutAnotherFakeMessageNotificationHandler());

            FakeMessage fakeMessage = null;

            Assert.Throws<ArgumentNullException>(() => sut[fakeMessage]);
        }

        [Fact]
        public void Dado_UmaInstanciaDaFakeMessageRegistradaNaServiceCollection_Se_TentarPegarOTopico_Entao_RetornaOTopicoQueEstaNaConstanteNaClasse()
        {
            var fakeMessage = new FakeMessage("", "");
            var expected = FakeMessage.TOPIC_NAME;

            var sut = new KafkaTopicCache(CreateServiceCollectionWithoutAnotherFakeMessageNotificationHandler());

            Assert.Equal(expected, sut[fakeMessage]);
        }

        [Fact]
        public void Dado_UmaInstanciaDaAnotherFakeMessageNaoRegistradaNaServiceCollection_Se_TentarPegarOTopico_Entao_EstouraKeyNotFoundException()
        {
            var fakeMessage = new AnotherFakeMessage("", "");

            var sut = new KafkaTopicCache(CreateServiceCollectionWithoutAnotherFakeMessageNotificationHandler());

            Assert.Throws<KeyNotFoundException>(() => sut[fakeMessage])
                .Message.Should().Be($"Não existe um tópico registrado para a mensagem do tipo '{typeof(AnotherFakeMessage).FullName}'");
        }

        [Fact]
        public void Dado_UmTipoQueNaoHerdaDeIMessage_Se_TentarPegarOTopico_Entao_EstouraKeyNotFoundException()
        {
            var sut = new KafkaTopicCache(CreateServiceCollectionWithoutAnotherFakeMessageNotificationHandler());

            var type = sut.GetType();

            Assert.Throws<KeyNotFoundException>(() => sut[type])
                .Message.Should().Be($"Não existe um tópico registrado para a mensagem do tipo '{type.FullName}'");
        }

        [Fact]
        public void Dado_UmTipoQueHerdeDeIMessageEEstaRegistradoNoServiceCollection_Se_TentarPegarOTopico_Entao_RetornaOTopicoQueEstaNaConstanteNaClasse()
        {
            var sut = new KafkaTopicCache(CreateServiceCollectionWithoutAnotherFakeMessageNotificationHandler());

            var type = typeof(FakeMessage);

            var expected = FakeMessage.TOPIC_NAME;

            Assert.Equal(expected, sut[type]);
        }

        [Fact]
        public void Dado_UmTipoQueHerdeDeIMessageENaoEstaRegistradoNoServiceCollection_Se_TentarPegarOTopico_Entao_EstouraKeyNotFoundException()
        {
            var sut = new KafkaTopicCache(CreateServiceCollectionWithoutAnotherFakeMessageNotificationHandler());

            var type = typeof(AnotherFakeMessage);

            Assert.Throws<KeyNotFoundException>(() => sut[type])
                .Message.Should().Be($"Não existe um tópico registrado para a mensagem do tipo '{typeof(AnotherFakeMessage).FullName}'");
        }

        [Fact]
        public void Dado_UmTopicoQueExistaClasseAssociadaEEstaRegistradoNoServiceCollection_Se_TentarPegarOTopico_Entao_RetornaAListaDeClasses()
        {
            var sut = new KafkaTopicCache(CreateServiceCollectionWithoutAnotherFakeMessageNotificationHandler());

            var type = typeof(FakeMessage);

            var expected = new List<Type> { type };

            var actual = sut[FakeMessage.TOPIC_NAME];

            expected.Should().BeEquivalentTo(actual);
        }

        [Fact]
        public void Dado_UmTopicoQueExistaClasseAssociadaENaoEstaRegistradoNoServiceCollection_Se_TentarPegarOTopico_Entao_EstouraKeyNotFoundException()
        {
            var sut = new KafkaTopicCache(CreateServiceCollectionWithoutAnotherFakeMessageNotificationHandler());

            Assert.Throws<KeyNotFoundException>(() => sut[AnotherFakeMessage.TOPIC_NAME])
                .Message.Should().Be($"Não existem mensagens para o tópico '{AnotherFakeMessage.TOPIC_NAME}'");
        }
    }
}

using FluentAssertions;
using GuimoSoft.Bus.Abstractions.Consumer;
using GuimoSoft.Bus.Tests.Fakes;
using System;
using Xunit;

namespace GuimoSoft.Bus.Tests.Common
{
    public class MessageNotificationTests
    {
        [Fact]
        public void Se_FakeMessageCriada_Entao_MessageNotificationCriada()
        {
            var fakeMessage = new FakeMessage("some-key", "some-property");

            var sut = new MessageNotification<FakeMessage>(fakeMessage);

            fakeMessage.Should().BeEquivalentTo((FakeMessage)sut);
        }

        [Fact]
        public void Se_FakeMessageCriada_Entao_ConversaoExplicitaParaMessageNotificationFunciona()
        {
            var fakeMessage = new FakeMessage("some-key", "some-property");

            var sut = (MessageNotification<FakeMessage>)fakeMessage;

            fakeMessage.Should().BeEquivalentTo((FakeMessage)sut);
        }

        [Fact]
        public void Se_FakeMessageNull_Entao_ConversaoExplicitaParaMessageNotificationEstoureArgumentNullException()
        {
            FakeMessage fakeMessage = null;

            Assert.Throws<ArgumentNullException>(() =>
            {
                var sut = (MessageNotification<FakeMessage>)fakeMessage;
            });
        }

        [Fact]
        public void Se_MessageNotificationIsNull_Entao_ConversaoExplicitaParaFakeMessageRetornaDefault()
        {
            MessageNotification<FakeMessage> sut = null;

            Assert.Null((FakeMessage)sut);
        }

        [Fact]
        public void Se_MessageNotificationCriado_Entao_EqualsComNuloIsFalse()
        {
            var fakeMessage = new FakeMessage("some-key", "some-property");
            FakeMessage nullFakeMessage = null;

            var sut = new MessageNotification<FakeMessage>(fakeMessage);

            Assert.False(sut.Equals(nullFakeMessage));
        }

        [Fact]
        public void Se_MessageNotificationCriadoComFakeMessage_Entao_EqualsComMessageNotificationCriadoComOutroFakeMessageIsFalse()
        {
            var fakeMessage1 = new FakeMessage("some-key", "some-property");
            var fakeMessage2 = new FakeMessage("another-key", "another-property");

            var anotherNotification = (MessageNotification<FakeMessage>)fakeMessage2;

            var sut = new MessageNotification<FakeMessage>(fakeMessage1);

            Assert.False(anotherNotification.Equals(sut));
        }

        [Fact]
        public void Se_MessageNotificationCriadoComFakeMessage_Entao_EqualsComMessageNotificationCriadoComOMesmoFakeMessageIsTrue()
        {
            var fakeMessage = new FakeMessage("some-key", "some-property");

            var anotherNotification = (MessageNotification<FakeMessage>)fakeMessage;

            var sut = new MessageNotification<FakeMessage>(fakeMessage);

            Assert.True(anotherNotification.Equals(sut));
        }

        [Fact]
        public void Se_MessageNotificationCriadoComFakeMessage_Entao_EqualsComOutroFakeMessageIsFalse()
        {
            var fakeMessage1 = new FakeMessage("some-key", "some-property");
            var fakeMessage2 = new FakeMessage("another-key", "another-property");

            var sut = new MessageNotification<FakeMessage>(fakeMessage1);

            Assert.False(sut.Equals(fakeMessage2));
        }

        [Fact]
        public void Se_MessageNotificationCriadoComFakeMessage_Entao_EqualsComOMesmoFakeMessageIsTrue()
        {
            var fakeMessage = new FakeMessage("some-key", "some-property");

            var sut = new MessageNotification<FakeMessage>(fakeMessage);

            Assert.True(sut.Equals(fakeMessage));
        }
    }
}
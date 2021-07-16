using FluentAssertions;
using GuimoSoft.Core;
using GuimoSoft.Notifications.Tests.Fakes;
using System.Collections.Generic;
using Xunit;

namespace GuimoSoft.Notifications.Tests
{
    public class NotificationContextTests
    {
        [Fact]
        public void Dado_UmaNotificacao_Se_AdicionarNotificacao_Entao_AdicionaComSucesso()
        {
            var sut = new NotificationContext<FakeErrorCode>();

            sut.AddNotification("teste", "teste");

            sut.HasNotifications
                .Should().BeTrue();

            sut.Notifications
                .Should().HaveCount(1);

            sut = new NotificationContext<FakeErrorCode>();

            sut.AddNotification(new Notification("teste", "teste"));

            sut.HasNotifications
                .Should().BeTrue();

            sut.Notifications
                .Should().HaveCount(1);

            sut.AssociateErrorCode(FakeErrorCode.FakeErrorCode1);

            sut.ErrorCode
                .Should().Be(FakeErrorCode.FakeErrorCode1);

            sut = new NotificationContext<FakeErrorCode>();

            sut.AddNotifications(new List<Notification> { new Notification("teste", "teste") });

            sut.HasNotifications
                .Should().BeTrue();

            sut.Notifications
                .Should().HaveCount(1);

            sut.AssociateErrorCode(FakeErrorCode.FakeErrorCode2);

            sut.ErrorCode
                .Should().Be(FakeErrorCode.FakeErrorCode2);

            var resultado = sut.GetResult();

            resultado
                .Should().NotBeNull();
        }
    }
}

using FluentAssertions;
using Microsoft.OpenApi.Extensions;
using System.Collections.Generic;
using System.ComponentModel;
using GuimoSoft.Core.Tests.Fakes;
using Xunit;

namespace GuimoSoft.Core.Tests
{
    public class NotificationListTests
    {
        [Fact]
        public void NotificationFacts()
        {
            var expectedNotification = new Notification("teste", "teste", "test");

            var sut = new Notification(expectedNotification.Field, expectedNotification.Event, expectedNotification.Value);

            sut
                .Should().BeEquivalentTo(expectedNotification);
        }

        [Fact]
        public void NotificationResultFacts()
        {
            var expectedNotification = new Notification("teste", "teste", "test");

            var expectedNotificationResult = new NotificationResult<FakeErrorCode>(new NotificationList<FakeErrorCode>(FakeErrorCode.FakeErrorCode1, expectedNotification));

            var sut = new NotificationResult<FakeErrorCode>(expectedNotificationResult.Notifications);

            sut
                .Should().BeEquivalentTo(expectedNotificationResult);
        }

        [Fact]
        public void Dado_UmaNotificacao_Se_CriarNotificationList_Entao_NaoRetornaErro()
        {
            var sut = new NotificationList<FakeErrorCode>(FakeErrorCode.FakeErrorCode1);

            sut.ErrorCode
                .Should().Be(FakeErrorCode.FakeErrorCode1);

            sut.Event
                .Should().Be(FakeErrorCode.FakeErrorCode1.GetAttributeOfType<DescriptionAttribute>().Description);

            sut.Notifications
                .Should().BeEmpty();

            sut = new NotificationList<FakeErrorCode>(FakeErrorCode.FakeErrorCode2, new Notification("teste", "teste"));

            sut.ErrorCode
                .Should().Be(FakeErrorCode.FakeErrorCode2);

            sut.Event
                .Should().Be(FakeErrorCode.FakeErrorCode2.ToString());

            sut.Notifications
                .Should().HaveCount(1);

            sut = new NotificationList<FakeErrorCode>(FakeErrorCode.FakeErrorCode3, new List<Notification> { new Notification("teste", "teste") });

            sut.ErrorCode
                .Should().Be(FakeErrorCode.FakeErrorCode3);

            sut.Event
                .Should().Be(FakeErrorCode.FakeErrorCode3.GetAttributeOfType<DescriptionAttribute>().Description);

            sut.Notifications
                .Should().HaveCount(1);

            sut = new NotificationList<FakeErrorCode>(default, new List<Notification> { new Notification("teste", "teste") });

            sut.ErrorCode
                .Should().Be(FakeErrorCode.FakeErrorCode1);

            sut.Event
                .Should().Be(FakeErrorCode.FakeErrorCode1.GetAttributeOfType<DescriptionAttribute>().Description);

            sut.Notifications
                .Should().HaveCount(1);
        }
    }
}

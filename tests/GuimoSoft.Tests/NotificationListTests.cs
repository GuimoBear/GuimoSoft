﻿using FluentAssertions;
using Microsoft.OpenApi.Extensions;
using System.Collections.Generic;
using System.ComponentModel;
using GuimoSoft.Tests.Fakes;
using Xunit;

namespace GuimoSoft.Tests
{
    public class NotificationListTests
    {
        [Fact]
        public void Dado_UmaNotificacao_Se_CriarNotificationList_Entao_NaoRetornaErro()
        {
            var sut = new NotificationList<FakeErrorCode>(FakeErrorCode.FakeErrorCode1);

            sut.ErrorCode
                .Should().Be(FakeErrorCode.FakeErrorCode1);

            sut.Message
                .Should().Be(FakeErrorCode.FakeErrorCode1.GetAttributeOfType<DescriptionAttribute>().Description);

            sut.Notifications
                .Should().BeEmpty();

            sut = new NotificationList<FakeErrorCode>(FakeErrorCode.FakeErrorCode2, new Notification("teste", "teste"));

            sut.ErrorCode
                .Should().Be(FakeErrorCode.FakeErrorCode2);

            sut.Message
                .Should().Be(FakeErrorCode.FakeErrorCode2.ToString());

            sut.Notifications
                .Should().HaveCount(1);

            sut = new NotificationList<FakeErrorCode>(FakeErrorCode.FakeErrorCode3, new List<Notification> { new Notification("teste", "teste") });

            sut.ErrorCode
                .Should().Be(FakeErrorCode.FakeErrorCode3);

            sut.Message
                .Should().Be(FakeErrorCode.FakeErrorCode3.GetAttributeOfType<DescriptionAttribute>().Description);

            sut.Notifications
                .Should().HaveCount(1);
        }
    }
}

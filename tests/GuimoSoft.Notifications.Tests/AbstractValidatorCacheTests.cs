using FluentAssertions;
using Moq;
using System.Collections.Generic;
using System.Linq;
using GuimoSoft.Notifications.AspNetCore;
using GuimoSoft.Notifications.Interfaces;
using GuimoSoft.Notifications.Tests.Fakes;
using GuimoSoft.Notifications.Tests.Fixtures;
using Xunit;
using GuimoSoft.Core;

namespace GuimoSoft.Notifications.Tests
{
    [Collection(ValidatorsCacheFixtureCollection.FIXTURE_COLLECTION_NAME)]
    public class AbstractValidatorCacheTests
    {
        public AbstractValidatorCacheTests(ValidatorsCacheFixture _) { }

        [Fact]
        public void When_FindValidators_Then_RegisterExecutorForFakeEntityValidator()
        {
            AbstractValidatorCache<FakeErrorCode>._validatorsCache
                .Should().ContainKey(typeof(FakeEntity));

            var executors = AbstractValidatorCache<FakeErrorCode>._validatorsCache[typeof(FakeEntity)]?.ToList();

            executors
                .Should().NotBeNullOrEmpty();

            executors[0]
                .Should().BeOfType<ValidationExecutor<FakeErrorCode, FakeEntity>>();

            var executor = executors[0] as ValidationExecutor<FakeErrorCode, FakeEntity>;

            var moqNotificationContext = new Mock<INotificationContext<FakeErrorCode>>();

            var notifications = new List<Notification>();

            moqNotificationContext
                .Setup(x => x.AddNotifications(It.IsAny<IEnumerable<Notification>>()))
                .Callback<IEnumerable<Notification>>(notifications.AddRange);

            executor.Validate(new FakeEntity(), moqNotificationContext.Object);

            moqNotificationContext
                .Verify(x => x.AddNotifications(It.IsAny<IEnumerable<Notification>>()), Times.Once);

            moqNotificationContext
                .Verify(x => x.AssociateErrorCode(FakeErrorCode.InvalidFakeEntity), Times.Once);

            notifications
                .Should().HaveCount(1);
        }

        [Fact]
        public void When_Get_Then_ReturnsValidExecutor()
        {
            var executors = AbstractValidatorCache<FakeErrorCode>.Get(typeof(FakeEntity))?.ToList();

            executors
                .Should().NotBeNullOrEmpty();

            executors[0]
                .Should().BeOfType<ValidationExecutor<FakeErrorCode, FakeEntity>>();

            var moqNotificationContext = new Mock<INotificationContext<FakeErrorCode>>();

            var notifications = new List<Notification>();

            moqNotificationContext
                .Setup(x => x.AddNotifications(It.IsAny<IEnumerable<Notification>>()))
                .Callback<IEnumerable<Notification>>(notifications.AddRange);

            var executor = executors[0] as ValidationExecutor<FakeErrorCode, FakeEntity>;

            executor.Validate(new FakeEntity(), moqNotificationContext.Object);

            moqNotificationContext
                .Verify(x => x.AddNotifications(It.IsAny<IEnumerable<Notification>>()), Times.Once);

            moqNotificationContext
                .Verify(x => x.AssociateErrorCode(FakeErrorCode.InvalidFakeEntity), Times.Once);

            notifications
                .Should().HaveCount(1);
        }
    }
}

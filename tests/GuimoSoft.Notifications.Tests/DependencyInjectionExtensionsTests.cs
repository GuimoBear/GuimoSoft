using FluentAssertions;
using GuimoSoft.Notifications.AspNetCore;
using GuimoSoft.Notifications.Interfaces;
using GuimoSoft.Notifications.Tests.Fakes;
using GuimoSoft.Notifications.Tests.Fixtures;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using Xunit;

namespace GuimoSoft.Notifications.Tests
{
    [Collection(ValidatorsCacheFixtureCollection.FIXTURE_COLLECTION_NAME)]
    public class DependencyInjectionExtensionsTests
    {
        public DependencyInjectionExtensionsTests(ValidatorsCacheFixture _) { }

        [Fact]
        public void AddNotificationContextFacts()
        {
            var services = new ServiceCollection();

            services.AddNotificationContext<FakeErrorCode>();

            services.FirstOrDefault(sd => sd.ServiceType == typeof(INotificationContext<FakeErrorCode>) &&
                                          sd.ImplementationType == typeof(NotificationContext<FakeErrorCode>) &&
                                          sd.Lifetime == ServiceLifetime.Scoped)
                .Should().NotBeNull();
        }

        [Fact]
        public void AddNotificationFilterFacts()
        {
            var options = new MvcOptions();

            options.AddNotificationFilter<FakeErrorCode>();

            options.Filters
                .Should().HaveCount(1);

            var filter = options.Filters.FirstOrDefault() as TypeFilterAttribute;

            filter
                .Should().NotBeNull();

            filter.ImplementationType
                .Should().Be(typeof(NotificationActionFilter<FakeErrorCode>));
        }
    }
}

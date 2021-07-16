using GuimoSoft.Notifications.AspNetCore;
using GuimoSoft.Notifications.Tests.Fakes;
using GuimoSoft.Notifications.Tests.Fixtures;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace GuimoSoft.Notifications.Tests
{
    [Collection(ValidatorsCacheFixtureCollection.FIXTURE_COLLECTION_NAME)]
    public class NotificationActionFilterTests
    {
        public NotificationActionFilterTests(ValidatorsCacheFixture _) { }

        [Fact]
        public async Task OnActionExecutionAsyncWithInvalidEntity()
        {
            var sut = new NotificationActionFilter<FakeErrorCode>();

            var modelState = new ModelStateDictionary();
            modelState.AddModelError("name", "invalid");

            var context = new DefaultHttpContext();

            var services = new ServiceCollection();

            services.AddNotificationContext<FakeErrorCode>(typeof(NotificationActionFilterTests).Assembly);

            using var scope = services.BuildServiceProvider().CreateScope();

            context.RequestServices = scope.ServiceProvider;

            var actionContext = new ActionContext(
                context,
                Mock.Of<RouteData>(),
                Mock.Of<ActionDescriptor>(),
                modelState
            );

            var actionExecutingContext = new ActionExecutingContext(
                actionContext,
                new List<IFilterMetadata>(),
                new Dictionary<string, object>()
                {
                    { "", new FakeEntity() }
                },
                Mock.Of<FakeController>()
            );

            var actionExecutedContext = new ActionExecutedContext(actionContext, new List<IFilterMetadata>(), Mock.Of<FakeController>());

            await sut.OnActionExecutionAsync(actionExecutingContext, () => throw new System.Exception("A action não deveria ser executada"));
        }


        [Fact]
        public async Task OnActionExecutionAsyncWithValidEntity()
        {
            var sut = new NotificationActionFilter<FakeErrorCode>();

            var modelState = new ModelStateDictionary();
            modelState.AddModelError("name", "invalid");

            var context = new DefaultHttpContext();

            var services = new ServiceCollection();

            services.AddNotificationContext<FakeErrorCode>(typeof(NotificationActionFilterTests).Assembly);

            using var scope = services.BuildServiceProvider().CreateScope();

            context.RequestServices = scope.ServiceProvider;

            var actionContext = new ActionContext(
                context,
                Mock.Of<RouteData>(),
                Mock.Of<ActionDescriptor>(),
                modelState
            );

            var actionExecutingContext = new ActionExecutingContext(
                actionContext,
                new List<IFilterMetadata>(),
                new Dictionary<string, object>()
                {
                    { "", new FakeEntity("teste") }
                },
                Mock.Of<FakeController>()
            );

            var actionExecutedContext = new ActionExecutedContext(actionContext, new List<IFilterMetadata>(), Mock.Of<FakeController>());

            await sut.OnActionExecutionAsync(actionExecutingContext, () => Task.FromResult(actionExecutedContext));
        }
    }
}

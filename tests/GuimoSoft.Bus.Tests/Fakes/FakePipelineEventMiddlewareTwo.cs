using System;
using System.Threading.Tasks;
using GuimoSoft.Bus.Abstractions;
using GuimoSoft.Bus.Abstractions.Consumer;

namespace GuimoSoft.Bus.Tests.Fakes
{
    public class FakePipelineEventMiddlewareTwo : IEventMiddleware<FakePipelineEvent>
    {
        public const string Name = nameof(FakePipelineEventMiddlewareTwo);

        public async Task InvokeAsync(ConsumeContext<FakePipelineEvent> context, Func<Task> next)
        {
            context.Event.MiddlewareNames.Add(Name);
            context.Items.Add(Name, true);
            if (Name.Equals(context.Event.LastMiddlewareToRun))
                return;
            await next();
        }
    }
}

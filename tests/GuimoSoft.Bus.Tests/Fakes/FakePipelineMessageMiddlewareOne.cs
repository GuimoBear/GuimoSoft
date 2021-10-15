using FluentAssertions;
using System;
using System.Linq;
using System.Threading.Tasks;
using GuimoSoft.Bus.Abstractions;
using GuimoSoft.Bus.Abstractions.Consumer;

namespace GuimoSoft.Bus.Tests.Fakes
{
    public class FakePipelineEventMiddlewareOne : IEventMiddleware<FakePipelineEvent>
    {
        public const string Name = nameof(FakePipelineEventMiddlewareOne);

        public async Task InvokeAsync(ConsumeContext<FakePipelineEvent> context, Func<Task> next)
        {
            Console.WriteLine(context.Informations.Bus);
            Console.WriteLine(context.Informations.Switch);
            Console.WriteLine(context.Informations.Endpoint);
            Console.WriteLine(string.Join(", ", context.Informations.Headers.Select(header => $"{header.Key} - {header.Value}")));

            context.Event.MiddlewareNames.Add(Name);
            context.Items.Add(Name, true);
            if (Name.Equals(context.Event.LastMiddlewareToRun))
                return;
            await next();
        }
    }
}

using System;
using System.Threading.Tasks;
using GuimoSoft.MessageBroker.Abstractions;
using GuimoSoft.MessageBroker.Kafka.Consumer;

namespace GuimoSoft.MessageBroker.Kafka.Tests.Fakes
{
    public class FakePipelineMessageMiddlewareTwo : IMessageMiddleware<FakePipelineMessage>
    {
        public const string Name = nameof(FakePipelineMessageMiddlewareTwo);

        public async Task InvokeAsync(ConsumptionContext<FakePipelineMessage> context, Func<Task> next)
        {
            context.Message.MiddlewareNames.Add(Name);
            context.Items.Add(Name, true);
            if (Name.Equals(context.Message.LastMiddlewareToRun))
                return;
            await next();
        }
    }
}

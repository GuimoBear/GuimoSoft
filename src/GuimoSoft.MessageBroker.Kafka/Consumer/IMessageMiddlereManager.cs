using System;
using GuimoSoft.MessageBroker.Abstractions;

namespace GuimoSoft.MessageBroker.Kafka.Consumer
{
    public interface IMessageMiddlereExecutorProvider
    {
        Pipeline GetPipeline(Type messageType);
    }

    public interface IMessageMiddlewareRegister
    {
        void Register<TMessage, TType>()
            where TMessage : IMessage
            where TType : class, IMessageMiddleware<TMessage>;

        void Register<TMessage, TType>(Func<IServiceProvider, TType> factory)
            where TMessage : IMessage
            where TType : class, IMessageMiddleware<TMessage>;
    }

    public interface IMessageMiddlereManager : IMessageMiddlereExecutorProvider, IMessageMiddlewareRegister
    {
    }
}

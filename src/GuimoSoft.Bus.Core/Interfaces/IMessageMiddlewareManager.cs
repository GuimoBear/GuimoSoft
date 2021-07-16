using GuimoSoft.Bus.Abstractions;
using GuimoSoft.Bus.Abstractions.Consumer;
using System;

namespace GuimoSoft.Bus.Core.Interfaces
{
    internal interface IMessageMiddlewareExecutorProvider
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

    internal interface IMessageMiddlewareManager : IMessageMiddlewareExecutorProvider, IMessageMiddlewareRegister
    {
    }
}

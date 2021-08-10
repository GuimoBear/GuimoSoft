using Microsoft.Extensions.DependencyInjection;
using System;
using GuimoSoft.Bus.Abstractions;
using GuimoSoft.Bus.Abstractions.Consumer;
using GuimoSoft.Bus.Core.Internal;

namespace GuimoSoft.Bus.Core.Interfaces
{
    internal interface IMessageMiddlewareExecutorProvider
    {
        Pipeline GetPipeline(BusName brokerName, Enum @switch, Type messageType);
    }

    public interface IMessageMiddlewareRegister
    {
        void Register<TMessage, TMiddleware>(BusName brokerName, Enum @switch, ServiceLifetime lifetime)
            where TMessage : IMessage
            where TMiddleware : class, IMessageMiddleware<TMessage>;

        void Register<TMessage, TMiddleware>(BusName brokerName, Enum @switch, Func<IServiceProvider, TMiddleware> factory, ServiceLifetime lifetime)
            where TMessage : IMessage
            where TMiddleware : class, IMessageMiddleware<TMessage>;
    }

    internal interface IMessageMiddlewareManager : IMessageMiddlewareExecutorProvider, IMessageMiddlewareRegister
    {
    }
}

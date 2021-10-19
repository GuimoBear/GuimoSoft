using GuimoSoft.Bus.Abstractions;
using GuimoSoft.Bus.Core.Internal.Middlewares;
using Sigil;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace GuimoSoft.Bus.Core.Logs
{
    internal static class DelegateCache
    {
        private static readonly ConcurrentDictionary<Type, TypedLogEventFactory> _typedBusLogEventFactories = new();
        private static readonly ConcurrentDictionary<Type, TypedExceptionEventFactory> _typedBusExceptionEventFactories = new();
        private static readonly ConcurrentDictionary<Type, EventDispatcherInvokeAsync> _typedEventDispatcherInvokeAsync = new();

        public static TypedLogEventFactory GetOrAddBusLogEventFactory(Type eventType)
            => _typedBusLogEventFactories.GetOrAdd(eventType, CreateBusLogEventFactory);

        public static TypedExceptionEventFactory GetOrAddBusExceptionEventFactory(Type eventType)
            => _typedBusExceptionEventFactories.GetOrAdd(eventType, CreateBusExceptionEventFactory);

        public static EventDispatcherInvokeAsync GetOrAddEventDispatcherInvokeAsync(Type eventType)
            => _typedEventDispatcherInvokeAsync.GetOrAdd(eventType, CreateEventDispatcherInvokeAsync);

        private static TypedLogEventFactory CreateBusLogEventFactory(Type eventType)
        {
            var logEventConstructor = typeof(BusTypedLogEvent<>).MakeGenericType(eventType)
                .GetConstructor(new Type[] { typeof(BusLogEvent), eventType });

            return Emit<TypedLogEventFactory>
                .NewDynamicMethod($"{eventType.Name}LogEvent_Ctor")
                .LoadArgument(0)
                .LoadArgument(1)
                .CastClass(eventType)
                .NewObject(logEventConstructor)
                .Return()
                .CreateDelegate();
        }

        private static TypedExceptionEventFactory CreateBusExceptionEventFactory(Type eventType)
        {
            var logEventConstructor = typeof(BusTypedExceptionEvent<>).MakeGenericType(eventType)
                .GetConstructor(new Type[] { typeof(BusExceptionEvent), eventType });

            return Emit<TypedExceptionEventFactory>
                .NewDynamicMethod($"{eventType.Name}ExceptionEvent_Ctor")
                .LoadArgument(0)
                .LoadArgument(1)
                .CastClass(eventType)
                .NewObject(logEventConstructor)
                .Return()
                .CreateDelegate();
        }

        private static EventDispatcherInvokeAsync CreateEventDispatcherInvokeAsync(Type eventType)
        {
            var eventDispatcherMiddlewareType = typeof(EventDispatcherMiddleware<>).MakeGenericType(eventType);
            var consumeContextType = typeof(ConsumeContext<>).MakeGenericType(eventType);
            var invokeAsyncMethodInfo = eventDispatcherMiddlewareType.GetMethod("InvokeAsync");

            return Emit<EventDispatcherInvokeAsync>
                .NewDynamicMethod($"{eventType.Name}EventDispatcher_InvokeAsync")
                .LoadArgument(0)
                .CastClass(eventDispatcherMiddlewareType)
                .LoadArgument(1)
                .CastClass(consumeContextType)
                .LoadArgument(2)
                .Call(invokeAsyncMethodInfo)
                .Return()
                .CreateDelegate();
        }
    }

    internal delegate object TypedLogEventFactory(BusLogEvent logEvent, object eventObject);
    internal delegate object TypedExceptionEventFactory(BusExceptionEvent exceptionEvent, object eventObject);

    internal delegate Task EventDispatcherInvokeAsync(object eventDispatcher, object consumeContext, Func<Task> next);
}

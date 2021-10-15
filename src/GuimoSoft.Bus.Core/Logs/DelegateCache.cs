using Sigil;
using System;
using System.Collections.Concurrent;

namespace GuimoSoft.Bus.Core.Logs
{
    internal static class DelegateCache
    {
        private static readonly ConcurrentDictionary<Type, TypedLogEventFactory> _typedBusLogEventFactories = new();
        private static readonly ConcurrentDictionary<Type, TypedExceptionEventFactory> _typedBusExceptionEventFactories = new();

        public static TypedLogEventFactory GetOrAddBusLogEventFactory(Type eventType)
            => _typedBusLogEventFactories.GetOrAdd(eventType, CreateBusLogEventFactory);

        public static TypedExceptionEventFactory GetOrAddBusExceptionEventFactory(Type eventType)
            => _typedBusExceptionEventFactories.GetOrAdd(eventType, CreateBusExceptionEventFactory);

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
    }

    internal delegate object TypedLogEventFactory(BusLogEvent logEvent, object eventObject);
    internal delegate object TypedExceptionEventFactory(BusExceptionEvent exceptionEvent, object eventObject);
}

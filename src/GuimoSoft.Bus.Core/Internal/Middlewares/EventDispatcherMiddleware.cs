using GuimoSoft.Bus.Abstractions;
using GuimoSoft.Bus.Abstractions.Consumer;
using Sigil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace GuimoSoft.Bus.Core.Internal.Middlewares
{
    internal abstract class EventDispatcherMiddlewareBase
    {
        protected static readonly Type _eventHandlerGenericTypeDefinition = typeof(IEventHandler<>);
        public IEnumerable<Type> HandlerTypes { get; protected set; }
    }

    internal sealed class EventDispatcherMiddleware<TEvent> : EventDispatcherMiddlewareBase, IEventMiddleware<TEvent>
        where TEvent : IEvent
    {
        private delegate Task ILHandlerCall(object handler, TEvent @event, CancellationToken cancellationToken);

        private static readonly Type[] _eventHandlerHandleArgumentTypes = new Type[] { typeof(TEvent), typeof(CancellationToken) };

        private readonly IDictionary<Type, ILHandlerCall> _handlerCalls;

        public EventDispatcherMiddleware()
        {
            _handlerCalls = CreateHandleCalls();
            HandlerTypes = _handlerCalls.Keys;
        }

        public async Task InvokeAsync(ConsumeContext<TEvent> context, Func<Task> next)
        {
            foreach (var (handlerType, handlerCall) in _handlerCalls)
                await handlerCall(context.Services.GetService(handlerType), context.Event, context.CancellationToken).ConfigureAwait(false);
            await next();
        }

        #region Private content
        private static IDictionary<Type, ILHandlerCall> CreateHandleCalls()
        {
            var eventType = typeof(TEvent);
            var handlerTypes = GetHandlerTypes(eventType);

            var calls = new Dictionary<Type, ILHandlerCall>();
            foreach (var handlerType in handlerTypes)
            {
                var handleMethod = GetHandleMethod(handlerType);

                var emitter = CreateEmitter(handlerType, handleMethod);

                calls.Add(handlerType, emitter.CreateDelegate());
            }
            return calls;
        }

        private static IEnumerable<Type> GetHandlerTypes(Type eventType)
        {
            var types = Singletons
                   .GetAssemblies()
                   .SelectMany(a => a.GetTypes())
                   .Where(type =>
                          type
                              .GetInterfaces()
                              .Any(@int =>
                              {
                                  if (@int.IsGenericType && @int.GetGenericTypeDefinition().Equals(_eventHandlerGenericTypeDefinition))
                                  {
                                      var generic = @int.GetGenericArguments()[0];
                                      return generic.Equals(eventType) || generic.IsAssignableFrom(eventType);
                                  }
                                  return false;
                              }))
                   .ToList();
            return types;
        }

        private static MethodInfo GetHandleMethod(Type handlerType)
        {
            return handlerType.GetMethod(
                    nameof(IEventHandler<TEvent>.Handle),
                    BindingFlags.Public | BindingFlags.Instance,
                    Type.DefaultBinder,
                    _eventHandlerHandleArgumentTypes,
                    Array.Empty<ParameterModifier>());
        }

        private static Emit<ILHandlerCall> CreateEmitter(Type handlerType, MethodInfo handleMethod)
        {
            var eventHandlerGenericType = handlerType.GetInterface("IEventHandler`1").GetGenericArguments()[0];
            if (eventHandlerGenericType == typeof(TEvent))
            {
                return Emit<ILHandlerCall>
                        .NewDynamicMethod()
                        .LoadArgument(0)
                        .CastClass(handlerType)
                        .LoadArgument(1)
                        .LoadArgument(2)
                        .Call(handleMethod)
                        .Return();
            }
            else
            {
                var parentType = eventHandlerGenericType;
                return Emit<ILHandlerCall>
                        .NewDynamicMethod()
                        .LoadArgument(0)
                        .CastClass(handlerType)
                        .LoadArgument(1)
                        .CastClass(parentType)
                        .LoadArgument(2)
                        .Call(handleMethod)
                        .Return();
            }
        }
        #endregion
    }
}

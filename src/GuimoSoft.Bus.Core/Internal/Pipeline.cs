using GuimoSoft.Bus.Abstractions;
using GuimoSoft.Bus.Abstractions.Consumer;
using Sigil;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace GuimoSoft.Bus.Core.Internal
{
    internal class Pipeline
    {
        private readonly IReadOnlyList<Type> _middlewareTypes;

        private readonly Func<ConsumeContextBase, Task> _executor = null;

        public Pipeline(IReadOnlyList<Type> middlewareTypes, Type eventType)
        {
            if (middlewareTypes is null)
                throw new ArgumentNullException(nameof(middlewareTypes));
            if (!typeof(IEvent).IsAssignableFrom(eventType))
                throw new ArgumentException($"{eventType.Name} deve implementar a interface {nameof(IEvent)}");

            var middlewareType = typeof(IEventMiddleware<>).MakeGenericType(eventType);
            if (middlewareTypes.Any(mt => !middlewareType.IsAssignableFrom(mt)))
                throw new ArgumentException($"Todos os middlewares devem implementar a interface {middlewareType.Name}<{eventType.Name}>");

            _middlewareTypes = middlewareTypes.Reverse().ToList();

            _executor = GetExecutor(typeof(ConsumeContext<>).MakeGenericType(eventType));
        }

        public async Task<ConsumeContextBase> Execute(object @event, IServiceProvider services, ConsumeInformations informations, CancellationToken cancellationToken)
        {
            var ctx = CreateContext(@event.GetType(), @event, services, informations, cancellationToken);
            await _executor(ctx);
            return ctx;
        }

        private Func<ConsumeContextBase, Task> GetExecutor(Type contextType)
        {
            Func<ConsumeContextBase, Task> source = _ => Task.CompletedTask;
            foreach (var middlewareExecutor in CreateMiddlewareExecutors(contextType))
                source = CreatePipelineLevelExecutor(source, middlewareExecutor);
            return source;
        }

        private static Func<ConsumeContextBase, Task> CreatePipelineLevelExecutor(Func<ConsumeContextBase, Task> source, Func<ConsumeContextBase, Func<Task>, Task> current)
        {
            return context => current(context, () => source(context));
        }

        private static readonly ConcurrentDictionary<Type, Func<object, IServiceProvider, ConsumeInformations, CancellationToken, ConsumeContextBase>> _constructorDelegates
            = new ();

        internal static ConsumeContextBase CreateContext(Type eventType, object @event, IServiceProvider services, ConsumeInformations informations, CancellationToken cancellationToken)
        {
            if (!_constructorDelegates.TryGetValue(eventType, out var ctor))
            {
                var constructorInfo = typeof(ConsumeContext<>).MakeGenericType(eventType)
                    .GetConstructor(
                        BindingFlags.Instance | BindingFlags.CreateInstance | BindingFlags.Public,
                        Type.DefaultBinder,
                        new Type[] { eventType, typeof(IServiceProvider), typeof(ConsumeInformations), typeof(CancellationToken) },
                        Array.Empty<ParameterModifier>());

                ctor = Emit<Func<object, IServiceProvider, ConsumeInformations, CancellationToken, ConsumeContextBase>>.NewDynamicMethod($"{eventType.Name}_EventContext_Ctor")
                    .LoadArgument(0)
                    .CastClass(eventType)
                    .LoadArgument(1)
                    .LoadArgument(2)
                    .LoadArgument(3)
                    .NewObject(constructorInfo)
                    .Return()
                    .CreateDelegate();

                _constructorDelegates.TryAdd(eventType, ctor);
            }
            return ctor(@event, services, informations, cancellationToken);
        }

        private static readonly ConcurrentDictionary<Type, Func<object, ConsumeContextBase, Func<Task>, Task>> _middlewareExecutorsCache
            = new();

        private IEnumerable<Func<ConsumeContextBase, Func<Task>, Task>> CreateMiddlewareExecutors(Type eventContextType)
        {
            foreach (var middlewareType in _middlewareTypes)
            {
                if (!_middlewareExecutorsCache.TryGetValue(middlewareType, out var middlewareInvokeDelegate))
                {
                    var middlewareExecutor = middlewareType.GetMethod("InvokeAsync", new Type[] { eventContextType, typeof(Func<Task>) });

                    var emmiter = Emit<Func<object, ConsumeContextBase, Func<Task>, Task>>
                        .NewDynamicMethod($"{middlewareType.Name}_InvokeAsync")
                        .LoadArgument(0)
                        .CastClass(middlewareType)
                        .LoadArgument(1)
                        .CastClass(eventContextType)
                        .LoadArgument(2)
                        .Call(middlewareExecutor)
                        .Return();

                    middlewareInvokeDelegate = emmiter.CreateDelegate();

                    _middlewareExecutorsCache.TryAdd(middlewareType, middlewareInvokeDelegate);
                }
                yield return async (obj, next) => await middlewareInvokeDelegate(obj.Services.GetService(middlewareType), obj, next);
            }
        }
    }
}

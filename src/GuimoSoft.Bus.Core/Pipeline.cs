using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using GuimoSoft.Bus.Abstractions;

namespace GuimoSoft.Bus.Core
{
    internal class Pipeline
    {
        private readonly IReadOnlyList<Type> _middlewareTypes;

        public Pipeline(IReadOnlyList<Type> middlewareTypes)
        {
            if (middlewareTypes == default)
                throw new ArgumentNullException(nameof(middlewareTypes));
            _middlewareTypes = middlewareTypes.Reverse().ToList();
        }

        public async Task<object> Execute(Type messageType, object message, IServiceProvider services)
        {
            var ctx = CreateContext(message, services);
            Func<object, Task> source = _ => Task.CompletedTask;

            var middlewareExecutors = CreateMiddlewareExecutors(services, messageType);

            foreach (var middlewareExecutor in middlewareExecutors)
                source = CreatePipelineLevelExecutor(source, middlewareExecutor);
            await source(ctx);
            return ctx;
        }

        private static Func<object, Task> CreatePipelineLevelExecutor(Func<object, Task> source, Func<object, Func<Task>, Task> current)
        {
            return async context => await current(context, async () => await source(context));
        }

        private static readonly ConcurrentDictionary<Type, ConstructorInfo> _contextConstructorsCache
            = new();

        private static readonly ConcurrentDictionary<Type, MethodInfo> _middlewareExecutorsCache
            = new();

        private static object CreateContext(object message, IServiceProvider services)
        {
            var messageType = message.GetType();
            if (!_contextConstructorsCache.TryGetValue(messageType, out var constructor))
            {
                var messageContextType = typeof(ConsumptionContext<>).MakeGenericType(messageType);
                var parameters = new Type[] { messageType, typeof(IServiceProvider) };
                constructor = messageContextType.GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, parameters, null);
                _contextConstructorsCache.TryAdd(messageType, constructor);
            }
            return constructor.Invoke(new object[] { message, services });
        }

        private IEnumerable<Func<object, Func<Task>, Task>> CreateMiddlewareExecutors(IServiceProvider services, Type messageType)
        {
            var messageContextType = typeof(ConsumptionContext<>).MakeGenericType(messageType);
            foreach (var middlewareType in _middlewareTypes)
            {
                if (!_middlewareExecutorsCache.TryGetValue(middlewareType, out var middlewareExecutor))
                {
                    middlewareExecutor = middlewareType.GetMethod("InvokeAsync", new Type[] { messageContextType, typeof(Func<Task>) });
                    _middlewareExecutorsCache.TryAdd(middlewareType, middlewareExecutor);
                }
                yield return async (obj, next) => await (middlewareExecutor.Invoke(services.GetService(middlewareType), new object[] { obj, next }) as Task);
            }
        }
    }
}

using GuimoSoft.Bus.Abstractions;
using GuimoSoft.Bus.Abstractions.Consumer;
using GuimoSoft.Bus.Core.Internal.Middlewares;
using GuimoSoft.Bus.Core.Logs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace GuimoSoft.Bus.Core.Internal
{
    internal static class DependencyInjectionExtensions
    {
        public static IServiceCollection RegisterMediatorFromNewAssemblies(this IServiceCollection services, IEnumerable<Assembly> assemblies)
        {
            lock (Singletons._lock)
            {
                var registeredAssemblies = Singletons.GetRegisteredAssemblies();
                var unregisteredAssemblies = assemblies.Where(a => !registeredAssemblies.Contains(a)).ToList();
                if (unregisteredAssemblies.Count > 0)
                {
                    services.RegisterEventHandlers(unregisteredAssemblies);
                    services.DiscoveryAndRegisterTypedExceptionEventHandlers(unregisteredAssemblies);
                    services.DiscoveryAndRegisterTypedLogEventHandlers(unregisteredAssemblies);
                    unregisteredAssemblies.ForEach(registeredAssemblies.Add);
                }
                return services;
            }
        }

        private static void DiscoveryAndRegisterTypedExceptionEventHandlers(this IServiceCollection services, IEnumerable<Assembly> assemblies)
        {
            var registeredBusTypedExceptionEvents = GetTypesFromGenericNotificationOrEventHandlers(assemblies, typeof(BusTypedExceptionEvent<>));
            var registeredExceptionHAndlerTypes = Singletons.GetBusTypedExceptionEventContainingAnHandlerCollection();

            registeredBusTypedExceptionEvents
                .Where(eventType => !registeredExceptionHAndlerTypes.Contains(eventType))
                .ToList()
                .ForEach(eventType =>
                {
                    _ = DelegateCache.GetOrAddBusLogEventFactory(eventType);
                    _ = DelegateCache.GetOrAddEventDispatcherInvokeAsync(eventType);
                    registeredExceptionHAndlerTypes.Add(eventType);
                });

            services.RegisterEventHandlers(assemblies, typeof(BusExceptionEvent), typeof(BusTypedExceptionEvent<>));
        }

        private static void DiscoveryAndRegisterTypedLogEventHandlers(this IServiceCollection services, IEnumerable<Assembly> assemblies)
        {
            var registeredBusTypedLogEvents = GetTypesFromGenericNotificationOrEventHandlers(assemblies, typeof(BusTypedLogEvent<>));
            var registeredLogHAndlerTypes = Singletons.GetBusTypedLogEventContainingAnHandlerCollection();

            registeredBusTypedLogEvents
                .Where(eventType => !registeredLogHAndlerTypes.Contains(eventType))
                .ToList()
                .ForEach(eventType =>
                {
                    _ = DelegateCache.GetOrAddBusLogEventFactory(eventType);
                    _ = DelegateCache.GetOrAddEventDispatcherInvokeAsync(eventType);
                    registeredLogHAndlerTypes.Add(eventType);
                });

            services.RegisterEventHandlers(assemblies, typeof(BusLogEvent), typeof(BusTypedLogEvent<>));
        }

        private static void RegisterEventHandlers(this IServiceCollection services, IEnumerable<Assembly> assemblies)
        {
            foreach (var assembly in assemblies)
                RegisterHandlersIntoAssembly(services, assembly);
        }

        private static void RegisterEventHandlers(this IServiceCollection services, IEnumerable<Assembly> assemblies, Type eventType, Type generigEventType)
        {
            var handlerTypes = assemblies
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type =>
                    type.GetInterfaces()
                        .Any(@int =>
                            @int.IsGenericType &&
                            @int.GetGenericTypeDefinition().Equals(typeof(IEventHandler<>)) &&
                            (@int.GetGenericArguments()[0] == eventType ||
                             (@int.GetGenericArguments()[0].IsGenericType &&
                              @int.GetGenericArguments()[0].GetGenericTypeDefinition() == generigEventType))))
                .ToList();

            handlerTypes.ForEach(services.TryAddSingleton);

            var eventDispatcherType = typeof(EventDispatcherMiddleware<>).MakeGenericType(eventType);
            var sd = services.FirstOrDefault(sd => sd.ServiceType == eventDispatcherType);
            if (sd is not null)
                services.Remove(sd);
            services.AddSingleton(eventDispatcherType, Activator.CreateInstance(eventDispatcherType));

            handlerTypes
                .SelectMany(type => type.GetInterfaces())
                .Where(@int => @int.IsGenericType &&
                            @int.GetGenericTypeDefinition().Equals(typeof(IEventHandler<>)) &&
                            @int.GetGenericArguments()[0].IsGenericType)
                .Select(@int => @int.GetGenericArguments()[0])
                .ToList()
                .ForEach(handlerType =>
                {
                    var eventDispatcherType = typeof(EventDispatcherMiddleware<>).MakeGenericType(handlerType);
                    var sd = services.FirstOrDefault(sd => sd.ServiceType == eventDispatcherType);
                    if (sd is not null)
                        services.Remove(sd);
                    services.AddSingleton(eventDispatcherType, Activator.CreateInstance(eventDispatcherType));
                });
        }

        private static void RegisterHandlersIntoAssembly(IServiceCollection services, Assembly assembly)
        {
            var events = GeteventTypesIntoAssembly(assembly);
            foreach (var @event in events)
            {
                var eventDispatcherMiddlewareType = typeof(EventDispatcherMiddleware<>).MakeGenericType(@event);
                if (!services.Any(sd => sd.ImplementationType == eventDispatcherMiddlewareType))
                {
                    var dispatcherInstance = Activator.CreateInstance(eventDispatcherMiddlewareType);
                    foreach (var handlerType in (dispatcherInstance as EventDispatcherMiddlewareBase).HandlerTypes)
                        services.TryAddTransient(handlerType);
                    services.AddSingleton(eventDispatcherMiddlewareType, dispatcherInstance);
                }
            }
        }

        private static List<Type> GetTypesFromGenericNotificationOrEventHandlers(IEnumerable<Assembly> assemblies, Type genericNotificationTypeDefinition)
        {
            return assemblies
                .SelectMany(assembly => assembly.GetTypes())
                .SelectMany(type => type.GetInterfaces())
                .Where(intr => intr.IsGenericType &&
                               intr.GetGenericTypeDefinition().Equals(typeof(IEventHandler<>)))
                .Select(handler => handler.GetGenericArguments()[0])
                .Where(eventType => eventType.IsGenericType &&
                                      eventType.GetGenericTypeDefinition().Equals(genericNotificationTypeDefinition))
                .Select(eventType => eventType.GetGenericArguments()[0])
                .Distinct()
                .ToList();
        }

        private static IEnumerable<Type> GeteventTypesIntoAssembly(Assembly assembly)
        {
            return assembly
                .GetTypes()
                .Where(type => type.GetInterfaces().Any(@int => @int.Equals(typeof(IEvent))));
        }
    }
}

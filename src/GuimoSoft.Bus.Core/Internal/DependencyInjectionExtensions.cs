using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GuimoSoft.Bus.Core.Logs;
using GuimoSoft.Bus.Abstractions;
using Microsoft.Extensions.DependencyInjection.Extensions;

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
                    services.AddMediatR(unregisteredAssemblies.ToArray());
                    services.RegisterEventHandlers(unregisteredAssemblies);
                    DiscoveryAndRegisterTypedExceptionEventHandlers(unregisteredAssemblies);
                    DiscoveryAndRegisterTypedLogEventHandlers(unregisteredAssemblies);
                    unregisteredAssemblies.ForEach(registeredAssemblies.Add);
                }
                return services;
            }
        }

        private static void DiscoveryAndRegisterTypedExceptionEventHandlers(IEnumerable<Assembly> assemblies)
        {
            var registeredBusTypedExceptionEvents = GetTypesFromGenericNotificationHandlers(assemblies, typeof(BusTypedExceptionEvent<>));
            var registeredExceptionHAndlerTypes = Singletons.GetBusTypedExceptionEventContainingAnHandlerCollection();

            registeredBusTypedExceptionEvents
                .Where(eventType => !registeredExceptionHAndlerTypes.Contains(eventType))
                .ToList()
                .ForEach(eventType =>
                {
                    _ = DelegateCache.GetOrAddBusLogEventFactory(eventType);
                    registeredExceptionHAndlerTypes.Add(eventType);
                });
        }

        private static void DiscoveryAndRegisterTypedLogEventHandlers(IEnumerable<Assembly> assemblies)
        {
            var registeredBusTypedLogEvents = GetTypesFromGenericNotificationHandlers(assemblies, typeof(BusTypedLogEvent<>));
            var registeredLogHAndlerTypes = Singletons.GetBusTypedLogEventContainingAnHandlerCollection();

            registeredBusTypedLogEvents
                .Where(eventType => !registeredLogHAndlerTypes.Contains(eventType))
                .ToList()
                .ForEach(eventType =>
                {
                    _ = DelegateCache.GetOrAddBusLogEventFactory(eventType);
                    registeredLogHAndlerTypes.Add(eventType);
                });
        }

        private static void RegisterEventHandlers(this IServiceCollection services, IEnumerable<Assembly> assemblies)
        {
            foreach (var assembly in assemblies)
                RegisterHandlersIntoAssembly(services, assembly);
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

        private static List<Type> GetTypesFromGenericNotificationHandlers(IEnumerable<Assembly> assemblies, Type genericNotificationTypeDefinition)
        {
            return assemblies
                .SelectMany(assembly => assembly.GetTypes())
                .SelectMany(type => type.GetInterfaces())
                .Where(intr => intr.IsGenericType &&
                               intr.GetGenericTypeDefinition().Equals(typeof(INotificationHandler<>)))
                .Select(handler => handler.GetGenericArguments()[0])
                .Where(eventType => eventType.IsGenericType &&
                                      eventType.GetGenericTypeDefinition().Equals(genericNotificationTypeDefinition))
                .Select(eventType => eventType.GetGenericArguments()[0])
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

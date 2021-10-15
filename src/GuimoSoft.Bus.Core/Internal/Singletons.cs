using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using GuimoSoft.Bus.Abstractions;
using GuimoSoft.Bus.Core.Interfaces;

namespace GuimoSoft.Bus.Core.Internal
{
    internal static class Singletons
    {
        public static readonly object _lock = new();

        private static BusSerializerManager _lazyBusSerializerManagerSingleton { get; set; } = default;
        private static EventMiddlewareManager _lazyEventMiddlewareManagerSingleton { get; set; } = default;
        private static EventTypeCache _lazyEventTypeCacheSingleton { get; set; } = default;
        private static DispatcherManager _lazyDispatcherManagerSingleton { get; set; } = default;
        private static ConcurrentDictionary<Type, object> _busOptionsDictionariesSingleton { get; } = new ();

        private static List<Assembly> _assemblyCollection { get; } = new ();

        private static List<Assembly> _registeredAssemblyCollection { get; } = new ();

        private static List<Type> _busTypedExceptionEventContainingAnHandler { get; } = new ();

        private static List<Type> _busTypedLogEventContainingAnHandler { get; } = new ();

        public static ICollection<Assembly> GetAssemblies()
            => _assemblyCollection;

        public static ICollection<Assembly> GetRegisteredAssemblies()
            => _registeredAssemblyCollection;

        public static ICollection<Type> GetBusTypedExceptionEventContainingAnHandlerCollection()
            => _busTypedExceptionEventContainingAnHandler;

        public static ICollection<Type> GetBusTypedLogEventContainingAnHandlerCollection()
            => _busTypedLogEventContainingAnHandler;

        public static BusSerializerManager TryRegisterAndGetBusSerializerManager(IServiceCollection services)
        {
            lock (_lock)
            {
                if (_lazyBusSerializerManagerSingleton is null)
                    _lazyBusSerializerManagerSingleton = new BusSerializerManager();

                services.TryAddSingleton<IBusSerializerManager>(_lazyBusSerializerManagerSingleton);

                return _lazyBusSerializerManagerSingleton;
            }
        }

        public static EventMiddlewareManager TryRegisterAndGeTEventMiddlewareManager(IServiceCollection services)
        {
            lock (_lock)
            {
                if (_lazyEventMiddlewareManagerSingleton is null)
                    _lazyEventMiddlewareManagerSingleton = new EventMiddlewareManager(services);

                services.TryAddSingleton<IEventMiddlewareManager>(_lazyEventMiddlewareManagerSingleton);
                services.TryAddSingleton(prov => prov.GetService(typeof(IEventMiddlewareManager)) as IEventMiddlewareExecutorProvider);
                services.TryAddSingleton(prov => prov.GetService(typeof(IEventMiddlewareManager)) as IEventMiddlewareRegister);

                return _lazyEventMiddlewareManagerSingleton;
            }
        }

        public static EventTypeCache TryRegisterAndGetEventTypeCache(IServiceCollection services)
        {
            lock (_lock)
            {
                if (_lazyEventTypeCacheSingleton is null)
                    _lazyEventTypeCacheSingleton = new EventTypeCache();

                services.TryAddSingleton<IEventTypeCache>(_lazyEventTypeCacheSingleton);

                return _lazyEventTypeCacheSingleton;
            }
        }

        public static DispatcherManager TryRegisterAndGetDispatcherManager(IServiceCollection services)
        {
            lock (_lock)
            {
                if (_lazyDispatcherManagerSingleton is null)
                    _lazyDispatcherManagerSingleton = new DispatcherManager(services);

                services.TryAddSingleton<IEventBus, EventBus>();
                services.TryAddSingleton<IDispatcherManager>(_lazyDispatcherManagerSingleton);

                return _lazyDispatcherManagerSingleton;
            }
        }

        public static BusOptionsDictionary<TOptions> TryRegisterAndGetBusOptionsDictionary<TOptions>(IServiceCollection services)
            where TOptions : class, new()
        {
            lock (_lock)
            {
                if (!_busOptionsDictionariesSingleton.TryGetValue(typeof(TOptions), out var dict))
                {
                    dict = new BusOptionsDictionary<TOptions>();
                    _busOptionsDictionariesSingleton.TryAdd(typeof(TOptions), dict);
                }
                var ret = dict as BusOptionsDictionary<TOptions>;
                services.TryAddSingleton<IBusOptionsDictionary<TOptions>>(ret);

                return ret;
            }
        }
    }
}

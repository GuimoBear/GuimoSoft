using Sigil;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using GuimoSoft.Bus.Core.Internal;
using GuimoSoft.Core.Serialization;
using GuimoSoft.Core.Serialization.Interfaces;

namespace GuimoSoft.Bus.Tests
{
    internal static class Utils
    {
        internal static readonly object Lock = new();

        internal static void ResetarEventSerializerManager()
        {
            typeof(EventSerializerManager)
                .GetField("_defaultSerializer", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(EventSerializerManager.Instance, JsonEventSerializer.Instance);

            var dict = typeof(EventSerializerManager)
                .GetField("_typedSerializers", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(EventSerializerManager.Instance);

            var method = typeof(ConcurrentDictionary<Type, IDefaultSerializer>)
                .GetMethod(nameof(IDictionary<Type, IDefaultSerializer>.Clear), BindingFlags.Instance | BindingFlags.Public);

            method.Invoke(dict, Array.Empty<object>());
        }

        internal static void ResetarSingletons()
        {
            lock (Singletons._lock)
            {
                lazyBusSerializerManagerSetter.Value(null);
                lazyEventMiddlewareManagerSetter.Value(null);
                lazyeventTypeCacheSetter.Value(null);
                lazyProducerManagerSetter.Value(null);
                lazyBusOptionsDictionariesCleaner.Value();
                lazyAssembliesCleaner.Value();
                lazyRegisteredAssembliesCleaner.Value();
                lazyTypedExceptionEventsCleaner.Value();
                lazyTypedLogEventsCleaner.Value();
            }
        }

        private static readonly Lazy<Action<BusSerializerManager>> lazyBusSerializerManagerSetter
            = new(CreateBusSerializerManagerSetter, System.Threading.LazyThreadSafetyMode.ExecutionAndPublication);

        private static readonly Lazy<Action<EventMiddlewareManager>> lazyEventMiddlewareManagerSetter
            = new(CreateEventMiddlewareManagerSetter, System.Threading.LazyThreadSafetyMode.ExecutionAndPublication);

        private static readonly Lazy<Action<EventTypeCache>> lazyeventTypeCacheSetter
            = new(CreateEventTypeCacheSetter, System.Threading.LazyThreadSafetyMode.ExecutionAndPublication);

        private static readonly Lazy<Action<DispatcherManager>> lazyProducerManagerSetter
            = new(CreateProducerManagerSetter, System.Threading.LazyThreadSafetyMode.ExecutionAndPublication);

        private static readonly Lazy<Action> lazyBusOptionsDictionariesCleaner
            = new(CreateBusOptionsDictionariesCleaner, System.Threading.LazyThreadSafetyMode.ExecutionAndPublication);

        private static readonly Lazy<Action> lazyAssembliesCleaner
            = new(CreateAssembliesCleaner, System.Threading.LazyThreadSafetyMode.ExecutionAndPublication);

        private static readonly Lazy<Action> lazyRegisteredAssembliesCleaner
            = new(CreateRegisteredAssembliesCleaner, System.Threading.LazyThreadSafetyMode.ExecutionAndPublication);

        private static readonly Lazy<Action> lazyTypedExceptionEventsCleaner
            = new(CreateTypedExceptionEventsCleaner, System.Threading.LazyThreadSafetyMode.ExecutionAndPublication);

        private static readonly Lazy<Action> lazyTypedLogEventsCleaner
            = new(CreateTypedLogEventsCleaner, System.Threading.LazyThreadSafetyMode.ExecutionAndPublication);

        private static Action<BusSerializerManager> CreateBusSerializerManagerSetter()
        {
            var propertyInfo = typeof(Singletons)
                .GetProperty("_lazyBusSerializerManagerSingleton", BindingFlags.Static | BindingFlags.NonPublic);

            return Emit<Action<BusSerializerManager>>
                .NewDynamicMethod("BusSerializerManagerSingleton_Setter")
                .LoadArgument(0)
                .Call(propertyInfo.GetSetMethod(true))
                .Return()
                .CreateDelegate();
        }

        private static Action<EventMiddlewareManager> CreateEventMiddlewareManagerSetter()
        {
            var propertyInfo = typeof(Singletons)
                .GetProperty("_lazyEventMiddlewareManagerSingleton", BindingFlags.Static | BindingFlags.NonPublic);

            return Emit<Action<EventMiddlewareManager>>
                .NewDynamicMethod("EventMiddlewareManagerSingleton_Setter")
                .LoadArgument(0)
                .Call(propertyInfo.GetSetMethod(true))
                .Return()
                .CreateDelegate();
        }

        private static Action<EventTypeCache> CreateEventTypeCacheSetter()
        {
            var propertyInfo = typeof(Singletons)
                .GetProperty("_lazyEventTypeCacheSingleton", BindingFlags.Static | BindingFlags.NonPublic);

            return Emit<Action<EventTypeCache>>
                .NewDynamicMethod("EventTypeCacheSingleton_Setter")
                .LoadArgument(0)
                .Call(propertyInfo.GetSetMethod(true))
                .Return()
                .CreateDelegate();
        }

        private static Action<DispatcherManager> CreateProducerManagerSetter()
        {
            var propertyInfo = typeof(Singletons)
                .GetProperty("_lazyDispatcherManagerSingleton", BindingFlags.Static | BindingFlags.NonPublic);

            return Emit<Action<DispatcherManager>>
                .NewDynamicMethod("DispatcherManagerSingleton_Setter")
                .LoadArgument(0)
                .Call(propertyInfo.GetSetMethod(true))
                .Return()
                .CreateDelegate();
        }

        private static Action CreateBusOptionsDictionariesCleaner()
        {
            var propertyInfo = typeof(Singletons)
                .GetProperty("_busOptionsDictionariesSingleton", BindingFlags.Static | BindingFlags.NonPublic);

            var cleanerMethodInfo = typeof(ConcurrentDictionary<Type, object>)
                .GetMethod(nameof(ConcurrentDictionary<Type, object>.Clear));

            return Emit<Action>
                .NewDynamicMethod("BusOptionsDictionaries_Cleaner")
                .Call(propertyInfo.GetGetMethod(true))
                .Call(cleanerMethodInfo)
                .Return()
                .CreateDelegate();
        }

        private static Action CreateAssembliesCleaner()
        {
            var propertyInfo = typeof(Singletons)
                .GetProperty("_assemblyCollection", BindingFlags.Static | BindingFlags.NonPublic);

            var cleanerMethodInfo = typeof(List<Assembly>)
                .GetMethod(nameof(List<Assembly>.Clear));

            return Emit<Action>
                .NewDynamicMethod("Assemblies_Cleaner")
                .Call(propertyInfo.GetGetMethod(true))
                .Call(cleanerMethodInfo)
                .Return()
                .CreateDelegate();
        }

        private static Action CreateRegisteredAssembliesCleaner()
        {
            var propertyInfo = typeof(Singletons)
                .GetProperty("_registeredAssemblyCollection", BindingFlags.Static | BindingFlags.NonPublic);

            var cleanerMethodInfo = typeof(List<Assembly>)
                .GetMethod(nameof(List<Assembly>.Clear));

            return Emit<Action>
                .NewDynamicMethod("RegisteredAssemblies_Cleaner")
                .Call(propertyInfo.GetGetMethod(true))
                .Call(cleanerMethodInfo)
                .Return()
                .CreateDelegate();
        }

        private static Action CreateTypedExceptionEventsCleaner()
        {
            var propertyInfo = typeof(Singletons)
                .GetProperty("_busTypedExceptionEventContainingAnHandler", BindingFlags.Static | BindingFlags.NonPublic);

            var cleanerMethodInfo = typeof(List<Type>)
                .GetMethod(nameof(List<Type>.Clear));

            return Emit<Action>
                .NewDynamicMethod("TypedExceptionEvents_Cleaner")
                .Call(propertyInfo.GetGetMethod(true))
                .Call(cleanerMethodInfo)
                .Return()
                .CreateDelegate();
        }

        private static Action CreateTypedLogEventsCleaner()
        {
            var propertyInfo = typeof(Singletons)
                .GetProperty("_busTypedLogEventContainingAnHandler", BindingFlags.Static | BindingFlags.NonPublic);

            var cleanerMethodInfo = typeof(List<Type>)
                .GetMethod(nameof(List<Type>.Clear));

            return Emit<Action>
                .NewDynamicMethod("TypedLogEvents_Cleaner")
                .Call(propertyInfo.GetGetMethod(true))
                .Call(cleanerMethodInfo)
                .Return()
                .CreateDelegate();
        }
    }
}

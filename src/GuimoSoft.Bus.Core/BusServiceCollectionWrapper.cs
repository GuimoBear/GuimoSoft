﻿using GuimoSoft.Bus.Abstractions;
using GuimoSoft.Bus.Core.Logs.Interfaces;
using GuimoSoft.Core.Serialization;
using GuimoSoft.Core.Serialization.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace GuimoSoft.Bus.Core
{
    public class BusServiceCollectionWrapper : IServiceCollection
    {
        protected readonly IServiceCollection _serviceCollection;

        public ServiceDescriptor this[int index]
        {
            get => _serviceCollection[index];
            set => _serviceCollection[index] = value;
        }

        public BusServiceCollectionWrapper(IServiceCollection serviceCollection)
        {
            _serviceCollection = serviceCollection;

            _serviceCollection.AddSingleton<IMessageSerializerManager>(MessageSerializerManager.Instance);
        }

        public virtual BusServiceCollectionWrapper WithDefaultSerializer(IDefaultSerializer defaultSerializer)
        {
            MessageSerializerManager.Instance.SetDefaultSerializer(defaultSerializer);
            return this;
        }

        public virtual BusServiceCollectionWrapper WithTypedSerializer<TMessage>(TypedSerializer<TMessage> serializer)
            where TMessage : IMessage
        {
            MessageSerializerManager.Instance.AddTypedSerializer(serializer);
            return this;
        }

        public virtual BusServiceCollectionWrapper WithLogger(IBusLogger logger)
        {
            if (logger is null)
                throw new ArgumentNullException(nameof(logger));
            RemoveBusLoggerServiceDescriptorIfExists();

            _serviceCollection.AddSingleton(logger);
            return this;
        }

        public virtual BusServiceCollectionWrapper WithLogger(Func<IServiceProvider, IBusLogger> loggerFactory)
        {
            if (loggerFactory is null)
                throw new ArgumentNullException(nameof(loggerFactory));
            RemoveBusLoggerServiceDescriptorIfExists();

            _serviceCollection.AddSingleton(loggerFactory);
            return this;
        }

        public int Count => _serviceCollection.Count;

        public bool IsReadOnly => _serviceCollection.IsReadOnly;

        public void Add(ServiceDescriptor item)
            => _serviceCollection.Add(item);

        public void Clear()
            => _serviceCollection.Clear();

        public bool Contains(ServiceDescriptor item)
            => _serviceCollection.Contains(item);

        public void CopyTo(ServiceDescriptor[] array, int arrayIndex)
            => _serviceCollection.CopyTo(array, arrayIndex);

        public IEnumerator<ServiceDescriptor> GetEnumerator()
            => _serviceCollection.GetEnumerator();

        public int IndexOf(ServiceDescriptor item)
            => _serviceCollection.IndexOf(item);

        public void Insert(int index, ServiceDescriptor item)
            => _serviceCollection.Insert(index, item);

        public bool Remove(ServiceDescriptor item)
            => _serviceCollection.Remove(item);

        public void RemoveAt(int index)
            => _serviceCollection.RemoveAt(index);

        IEnumerator IEnumerable.GetEnumerator()
            => _serviceCollection.GetEnumerator();

        protected void RemoveBusLoggerServiceDescriptorIfExists()
        {
            var sd = _serviceCollection.FirstOrDefault(sd => sd.ServiceType == typeof(IBusLogger));
            if (sd is not null)
                _serviceCollection.Remove(sd);

        }
    }
}
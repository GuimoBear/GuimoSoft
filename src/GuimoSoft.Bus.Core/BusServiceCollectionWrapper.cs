using Microsoft.Extensions.DependencyInjection;
using System.Collections;
using System.Collections.Generic;
using GuimoSoft.Bus.Abstractions;
using GuimoSoft.Bus.Core.Interfaces;
using GuimoSoft.Serialization;
using GuimoSoft.Serialization.Interfaces;

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
    }
}

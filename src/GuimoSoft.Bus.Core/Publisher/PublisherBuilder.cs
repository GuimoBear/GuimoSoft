using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Reflection;
using GuimoSoft.Bus.Abstractions;
using GuimoSoft.Bus.Core.Exceptions;
using GuimoSoft.Bus.Core.Internal;
using GuimoSoft.Bus.Core.Utils;
using GuimoSoft.Core.Serialization.Interfaces;

namespace GuimoSoft.Bus.Core.Publisher
{
    public sealed class PublisherBuilder<TOptions>
        where TOptions : class, new()
    {
        private readonly BusName _busName;
        private readonly Enum _switch;
        private readonly ICollection<Assembly> _assemblies;
        private readonly BusSerializerManager _busSerializerManager;
        private readonly EventTypeCache _eventTypesCache;
        private readonly BusOptionsDictionary<TOptions> _optionsDictionary;

        internal PublisherBuilder(BusName busName, Enum @switch, ICollection<Assembly> assemblies, IServiceCollection services)
        {
            _busName = busName;
            _switch = @switch;
            _assemblies = assemblies;

            _busSerializerManager = Singletons.TryRegisterAndGetBusSerializerManager(services);
            _eventTypesCache = Singletons.TryRegisterAndGetEventTypeCache(services);
            _optionsDictionary = Singletons.TryRegisterAndGetBusOptionsDictionary<TOptions>(services);
            ValidateBusOptions();
        }

        public PublisherBuilder<TOptions> ToServer(Action<TOptions> configure)
        {
            var config = new TOptions();
            configure(config);
            _optionsDictionary[_switch] = config;
            return this;
        }

        public PublisherBuilder<TOptions> WithDefaultSerializer(IDefaultSerializer defaultSerializer)
        {
            _busSerializerManager.SetDefaultSerializer(_busName, Finality.Produce, _switch, defaultSerializer);

            return this;
        }

        public PublisherBuilder<TOptions> AddAnotherAssemblies(params Assembly[] assemblies)
        {
            if (assemblies is not null)
                foreach (var assembly in assemblies)
                    _assemblies.TryAddAssembly(assembly);

            return this;
        }

        public EndpointPublisherBuilder<TOptions> Publish()
            => new EndpointPublisherBuilder<TOptions>(this, _busName, _switch, _busSerializerManager, _eventTypesCache, _assemblies);

        internal void ValidateAfterConfigured()
        {
            if (!_optionsDictionary.ContainsKey(_switch))
                throw new BusOptionsMissingException(_busName, _switch, typeof(TOptions));
        }

        private void ValidateBusOptions()
        {
            if (_optionsDictionary.ContainsKey(_switch))
                throw new BusAlreadyConfiguredException(_busName, _switch);
        }
    }
}

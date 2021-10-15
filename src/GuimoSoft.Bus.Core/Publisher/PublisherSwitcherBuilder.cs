using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Reflection;
using GuimoSoft.Bus.Abstractions;
using GuimoSoft.Bus.Core.Utils;

namespace GuimoSoft.Bus.Core.Publisher
{
    public sealed class PublisherSwitcherBuilder<TSwitch, TOptions>
        where TSwitch : struct, Enum
        where TOptions : class, new()
    {
        private readonly BusName _busName;
        private readonly ICollection<Assembly> _assemblies;
        private readonly IServiceCollection _services;

        private readonly List<PublisherBuilder<TOptions>> _builders;

        internal PublisherSwitcherBuilder(BusName busName, ICollection<Assembly> assemblies, IServiceCollection services)
        {
            _busName = busName;
            _assemblies = assemblies;
            _services = services;
            _builders = new();
        }

        public PublisherBuilder<TOptions> When(TSwitch @switch)
        {
            var builder = new PublisherBuilder<TOptions>(_busName, @switch, _assemblies, _services);
            _builders.Add(builder);
            return builder;
        }

        public PublisherSwitcherBuilder<TSwitch, TOptions> AddAnotherAssembliesToMediatR(params Assembly[] assemblies)
        {
            if (assemblies is not null)
                foreach (var assembly in assemblies)
                    _assemblies.TryAddAssembly(assembly);

            return this;
        }

        internal void ValidateAfterConfigured()
        {
            foreach (var builder in _builders)
                builder.ValidateAfterConfigured();
        }
    }
}

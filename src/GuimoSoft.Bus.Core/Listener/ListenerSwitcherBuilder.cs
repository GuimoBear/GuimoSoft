using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Reflection;
using GuimoSoft.Bus.Abstractions;
using GuimoSoft.Bus.Core.Utils;

namespace GuimoSoft.Bus.Core.Listener
{
    public sealed class ListenerSwitcherBuilder<TSwitch, TOptions>
        where TSwitch : struct, Enum
        where TOptions : class, new()
    {
        private readonly BusName _busName;
        private readonly ICollection<Assembly> _assemblies;
        private readonly IServiceCollection _services;

        private readonly List<ListenerBuilder<TOptions>> _builders;

        internal ListenerSwitcherBuilder(BusName busName, ICollection<Assembly> assemblies, IServiceCollection services)
        {
            _busName = busName;
            _assemblies = assemblies;
            _services = services;
            _builders = new();
        }

        public ListenerBuilder<TOptions> When(TSwitch @switch)
        {
            var builder = new ListenerBuilder<TOptions>(_busName, @switch, _assemblies, _services);
            _builders.Add(builder);
            return builder;
        }

        public ListenerSwitcherBuilder<TSwitch, TOptions> AddAnotherAssembliesToMediatR(params Assembly[] assemblies)
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

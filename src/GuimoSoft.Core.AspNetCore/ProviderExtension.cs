using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace GuimoSoft.Core
{
    public delegate Task<Tenant> AdditionalTenantProvider(HttpContext context);
    public delegate Task<CorrelationId> AdditionalCorrelationIdProvider(HttpContext context);
    public class ProviderExtension : IProviderExtension
    {
        internal static readonly ConcurrentBag<AdditionalTenantProvider> additionalTenantProvidersSingleton
            = new ConcurrentBag<AdditionalTenantProvider>();

        internal static readonly ConcurrentBag<AdditionalCorrelationIdProvider> additionalCorrelationIdProvidersSingleton
            = new ConcurrentBag<AdditionalCorrelationIdProvider>();

        private readonly IEnumerable<AdditionalTenantProvider> additionalTenantProviders;
        private readonly IEnumerable<AdditionalCorrelationIdProvider> additionalCorrelationIdProviders;

        internal ProviderExtension() 
        {
            additionalTenantProviders = new List<AdditionalTenantProvider>(additionalTenantProvidersSingleton);
            additionalCorrelationIdProviders = new List<AdditionalCorrelationIdProvider>(additionalCorrelationIdProvidersSingleton);
        }

        public static void UseAdditionalTenantProvider(AdditionalTenantProvider additionalTenantProvider)
        {
            additionalTenantProvidersSingleton.Add(additionalTenantProvider ?? throw new ArgumentNullException(nameof(additionalTenantProvider)));
        }

        public static void UseAdditionalCorrelationIdProvider(AdditionalCorrelationIdProvider additionalCorrelationIdProvider)
        {
            additionalCorrelationIdProvidersSingleton.Add(additionalCorrelationIdProvider ?? throw new ArgumentNullException(nameof(additionalCorrelationIdProvider)));
        }

        public async Task<Tenant> GetTenant([DisallowNull] HttpContext context)
        {
            foreach (var additionalTenantProvider in additionalTenantProviders)
            {
                var tenant = await additionalTenantProvider(context);
                if (tenant is not null)
                    return tenant;
            }
            return default;
        }

        public async Task<CorrelationId> GetCorrelationId([DisallowNull] HttpContext context)
        {
            foreach (var additionalCorrelationIdProvider in additionalCorrelationIdProviders)
            {
                var correlationId = await additionalCorrelationIdProvider(context);
                if (correlationId is not null)
                    return correlationId;
            }
            return default;
        }
    }
}

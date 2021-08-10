using Microsoft.AspNetCore.Http;
using GuimoSoft.Core.AspNetCore.Constants;
using GuimoSoft.Core.AspNetCore.Exceptions;
using GuimoSoft.Core.Providers.Interfaces;

namespace GuimoSoft.Core.AspNetCore
{
    public class TenantProvider : ITenantSetter, ITenantProvider
    {
        private readonly IHttpContextAccessor accessor;
        private readonly IProviderExtension providerExtension;

        public TenantProvider(IHttpContextAccessor accessor, IProviderExtension providerExtension)
        {
            this.accessor = accessor;
            this.providerExtension = providerExtension;
        }

        private Tenant tenant;
        private bool tenantAlteradoAnteriormente = false;

        public virtual Tenant Tenant
        {
            get => tenant;
        }

        public virtual void SetTenant(Tenant tenant)
            => AlterarTenantSeNaoEstiverSidoAlteradoAnteriormente(tenant);

        public Tenant Obter()
        {
            if (tenantAlteradoAnteriormente)
                return Tenant;
            if (TentarObterTenantNoHttpContext(out var tenant))
                return tenant;

            return providerExtension.GetTenant(accessor?.HttpContext)
                    .ConfigureAwait(false)
                    .GetAwaiter()
                    .GetResult() ?? "";
        }

        private bool TentarObterTenantNoHttpContext(out Tenant tenant)
        {
            return
                TentarObterPorHeaderOrigem(out tenant);
        }

        private bool TentarObterPorHeaderOrigem(out Tenant tenant)
        {
            if (accessor?.HttpContext?.Request is not null)
            {
                var strOrigem = accessor.HttpContext.Request.Headers[RequestConstants.ORIGEM_HEADER];

                if (!string.IsNullOrEmpty(strOrigem))
                {
                    tenant = strOrigem.ToString().ToLowerInvariant();
                    return true;
                }
            }
            tenant = default;
            return false;
        }

        private void AlterarTenantSeNaoEstiverSidoAlteradoAnteriormente(Tenant value)
        {
            if (!string.IsNullOrEmpty(value?.Value) && !tenantAlteradoAnteriormente)
            {
                tenantAlteradoAnteriormente = true;
                tenant = value;
            }
            else if (!string.IsNullOrEmpty(value?.Value) && !value.Equals(tenant))
                throw new TenantJaSetadoException(tenant, value);
        }
    }
}

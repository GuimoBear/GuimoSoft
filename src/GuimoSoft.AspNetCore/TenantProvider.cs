using Microsoft.AspNetCore.Http;
using GuimoSoft.AspNetCore.Constants;
using GuimoSoft.AspNetCore.Exceptions;
using GuimoSoft.Providers.Interfaces;

namespace GuimoSoft.AspNetCore
{
    public class TenantProvider : ITenantSetter, ITenantProvider
    {
        private readonly IHttpContextAccessor accessor;

        public TenantProvider(IHttpContextAccessor accessor)
        {
            this.accessor = accessor;
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
            return "";
        }

        private bool TentarObterTenantNoHttpContext(out Tenant tenant)
        {
            return
                TentarObterPorHeaderOrigem(out tenant);
        }

        private bool TentarObterPorHeaderOrigem(out Tenant tenant)
        {
            var strOrigem = accessor?.HttpContext?.Request?.Headers?[RequestConstants.ORIGEM_HEADER];

            if (!string.IsNullOrEmpty(strOrigem))
            {
                tenant = strOrigem.ToString().ToLowerInvariant();
                return true;
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
            else if (tenantAlteradoAnteriormente && !string.IsNullOrEmpty(value?.Value) && tenant != value)
                throw new TenantJaSetadoException(tenant, value);
        }
    }
}

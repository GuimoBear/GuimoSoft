using Microsoft.AspNetCore.Http;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace GuimoSoft.Core
{
    public interface IProviderExtension
    {
        Task<Tenant> GetTenant([DisallowNull] HttpContext context);
        Task<CorrelationId> GetCorrelationId([DisallowNull] HttpContext context);
    }
}

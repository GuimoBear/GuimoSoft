using Microsoft.AspNetCore.Http;
using System;
using GuimoSoft.Core.AspNetCore.Constants;
using GuimoSoft.Core.AspNetCore.Exceptions;
using GuimoSoft.Core.Providers.Interfaces;

namespace GuimoSoft.Core.AspNetCore
{
    public class CorrelationIdProvider : ICorrelationIdSetter, ICorrelationIdProvider
    {
        private readonly IHttpContextAccessor accessor;
        private readonly Lazy<CorrelationId> lazyCorrelationId;

        private CorrelationId _correlationId;
        private bool _correlationIdPreviouslySetted = false;
        public virtual CorrelationId CorrelationId
        {
            get => _correlationId;
        }

        public CorrelationIdProvider(IHttpContextAccessor accessor, IProviderExtension providerExtension)
        {
            this.accessor = accessor;
            lazyCorrelationId = new Lazy<CorrelationId>(() =>
            {
                if (_correlationIdPreviouslySetted)
                    return CorrelationId;
                if (accessor?.HttpContext?.Request is not null)
                {
                    var correlationId = accessor.HttpContext.Request.Headers[RequestConstants.CORRELATION_ID_HEADER].ToString();
                    if (string.IsNullOrEmpty(correlationId))
                        correlationId = providerExtension.GetCorrelationId(accessor.HttpContext)
                            .ConfigureAwait(false)
                            .GetAwaiter()
                            .GetResult();
                    return !string.IsNullOrEmpty(correlationId) ?
                        correlationId :
                        Guid.NewGuid().ToString();
                }
                return Guid.NewGuid().ToString();
            });
        }

        public virtual void SetCorrelationId(CorrelationId correlationId)
        {
            if (!_correlationIdPreviouslySetted)
            {
                _correlationIdPreviouslySetted = true;
                _correlationId = correlationId;
            }
            else if (_correlationId != correlationId)
                throw new CorrelationIdJaSetadoException(_correlationId, correlationId);
        }

        public CorrelationId Get()
        {
            return lazyCorrelationId.Value;
        }

        public void SetCorrelationIdInResponseHeader()
        {
            accessor?.HttpContext?.Response?.Headers?.Add(RequestConstants.CORRELATION_ID_HEADER, Get().ToString());
        }
    }
}

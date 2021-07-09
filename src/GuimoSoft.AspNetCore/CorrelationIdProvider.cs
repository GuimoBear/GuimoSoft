using Microsoft.AspNetCore.Http;
using System;
using GuimoSoft.AspNetCore.Constants;
using GuimoSoft.AspNetCore.Exceptions;
using GuimoSoft.Providers.Interfaces;

namespace GuimoSoft.AspNetCore
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

        public CorrelationIdProvider(IHttpContextAccessor accessor)
        {
            this.accessor = accessor;
            lazyCorrelationId = new Lazy<CorrelationId>(() =>
            {
                if (_correlationIdPreviouslySetted)
                    return CorrelationId;
                if (!(accessor?.HttpContext?.Request is null))
                {
                    var correlationId = accessor.HttpContext.Request.Headers[RequestConstants.CORRELATION_ID_HEADER].ToString();
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

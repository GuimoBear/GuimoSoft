using System.Collections.Generic;

namespace GuimoSoft.Logger
{
    public interface IApiLoggerContextAccessor
    {
        IDictionary<string, object> Context { get; set; }
    }
}

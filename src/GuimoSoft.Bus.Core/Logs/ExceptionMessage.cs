using Microsoft.Extensions.Logging;
using System;

namespace GuimoSoft.Bus.Core.Logs
{
    public class ExceptionMessage : LogMessage
    {
        public Exception Exception { get; init; }

        public ExceptionMessage(string message, LogLevel level) : base(message, level)
        {

        }
    }
}

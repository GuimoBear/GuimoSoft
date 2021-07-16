using GuimoSoft.Bus.Core.Logs.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace GuimoSoft.Bus.Core.Logs
{
    public class DefaultBusLogger : IBusLogger
    {
        private readonly ILogger<DefaultBusLogger> _logger;

        public DefaultBusLogger(ILogger<DefaultBusLogger> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public virtual Task LogAsync(LogMessage log)
        {
            if (log is not null)
                _logger.Log(log.Level, log.Message, log.Data);
            return Task.CompletedTask;
        }

        public virtual Task ExceptionAsync(ExceptionMessage exception)
        {
            if (exception is not null)
                _logger.Log(exception.Level, exception.Exception, exception.Message, exception.Data);
            return Task.CompletedTask;
        }
    }
}

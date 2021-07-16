using GuimoSoft.Bus.Core.Logs;
using GuimoSoft.Bus.Core.Logs.Interfaces;
using GuimoSoft.Logger;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace GuimoSoft.Examples.Bus.Kafka.Utils
{
    public class BusLogger : IBusLogger
    {
        private readonly IApiLogger<BusLogger> _logger;

        public BusLogger(IApiLogger<BusLogger> logger)
        {
            _logger = logger;
        }

        public Task ExceptionAsync(ExceptionMessage exception)
        {
            _logger
                .ComPropriedade(nameof(exception.Data), exception.Data)
                .ComPropriedade(nameof(exception.Level), exception.Level)
                .Erro(exception.Message, exception.Exception);
            return Task.CompletedTask;
        }

        public Task LogAsync(LogMessage log)
        {
            var builder = _logger
                .ComPropriedade(nameof(log.Data), log.Data)
                .ComPropriedade(nameof(log.Level), log.Level);

            switch (log.Level)
            {
                case LogLevel.Trace:
                    builder.Rastreio(log.Message);
                    break;
                case LogLevel.Debug:
                    builder.Depuracao(log.Message);
                    break;
                case LogLevel.Information:
                    builder.Informacao(log.Message);
                    break;
                case LogLevel.Warning:
                    builder.Atencao(log.Message);
                    break;
                case LogLevel.Error:
                    builder.Erro(log.Message);
                    break;
            }

            return Task.CompletedTask;
        }
    }
}

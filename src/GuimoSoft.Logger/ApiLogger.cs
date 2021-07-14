using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using GuimoSoft.Logger.Utils;

namespace GuimoSoft.Logger
{
    public class ApiLogger<TCategoryName> : ApiLoggerBase, IApiLogger<TCategoryName>
    {
        private readonly ILogger<TCategoryName> logger;
        private readonly IApiLoggerContextAccessor loggerContextAccessor;

        public ApiLogger(ILogger<TCategoryName> logger,
                         IApiLoggerContextAccessor loggerContextAccessor)
        {
            this.logger = logger;
            this.loggerContextAccessor = loggerContextAccessor;
        }

        public ILoggerBuilder ComPropriedade<T>(string key, [NotNull] T value)
        {
            var dicionarioDeLog = CriarDicionarioDeLog();
            KeyValuePair<string, Stopwatch> cronometro = default;
            if (!string.IsNullOrEmpty(key))
            {
                if (value is not Stopwatch stopwatch)
                    dicionarioDeLog.TryAdd(key, value);
                else
                    cronometro = new KeyValuePair<string, Stopwatch>(key, stopwatch);
            }
            return new LoggerBuilder(this, dicionarioDeLog, cronometro);
        }

        public ExpandoObject CriarDicionarioDeLog()
        {
            var log = new ExpandoObject();
            if (loggerContextAccessor?.Context is not null)
            {
                foreach (var (key, value) in loggerContextAccessor.Context)
                    log.TryAdd(key, value);
            }
            return log;
        }

        public void Rastreio(string mensagem)
        {
            if (string.IsNullOrWhiteSpace(mensagem))
                return;
            var log = CriarDicionarioDeLog();
            log.TryAdd(Constants.KEY_MESSAGE, mensagem);
            log.TryAdd(Constants.KEY_SEVERITY, ConvertLogLevelToString(LogLevel.Trace));
            LogLevelAccessor.LogLevel = LogLevel.Trace;
            logger.LogTrace(JsonConvert.SerializeObject(log));
        }
        public void Rastreio(ExpandoObject log)
        {
            if (log is null)
                return;
            log.TryAdd(Constants.KEY_SEVERITY, ConvertLogLevelToString(LogLevel.Trace));
            LogLevelAccessor.LogLevel = LogLevel.Trace;
            logger.LogTrace(JsonConvert.SerializeObject(log, SERIALIZER_SETTINGS));
        }
        public void Depuracao(string mensagem)
        {
            if (string.IsNullOrWhiteSpace(mensagem))
                return;
            var log = CriarDicionarioDeLog();
            log.TryAdd(Constants.KEY_MESSAGE, mensagem);
            log.TryAdd(Constants.KEY_SEVERITY, ConvertLogLevelToString(LogLevel.Debug));
            LogLevelAccessor.LogLevel = LogLevel.Debug;
            logger.LogDebug(JsonConvert.SerializeObject(log));
        }
        public void Depuracao(ExpandoObject log)
        {
            if (log is null)
                return;
            log.TryAdd(Constants.KEY_SEVERITY, ConvertLogLevelToString(LogLevel.Debug));
            LogLevelAccessor.LogLevel = LogLevel.Debug;
            logger.LogDebug(JsonConvert.SerializeObject(log, SERIALIZER_SETTINGS));
        }
        public void Informacao(string mensagem)
        {
            if (string.IsNullOrWhiteSpace(mensagem))
                return;
            var log = CriarDicionarioDeLog();
            log.TryAdd(Constants.KEY_MESSAGE, mensagem);
            log.TryAdd(Constants.KEY_SEVERITY, ConvertLogLevelToString(LogLevel.Information));
            LogLevelAccessor.LogLevel = LogLevel.Information;
            logger.LogInformation(JsonConvert.SerializeObject(log));
        }
        public void Informacao(ExpandoObject log)
        {
            if (log is null)
                return;
            log.TryAdd(Constants.KEY_SEVERITY, ConvertLogLevelToString(LogLevel.Information));
            LogLevelAccessor.LogLevel = LogLevel.Information;
            logger.LogInformation(JsonConvert.SerializeObject(log, SERIALIZER_SETTINGS));
        }
        public void Atencao(string mensagem)
        {
            if (string.IsNullOrWhiteSpace(mensagem))
                return;
            var log = CriarDicionarioDeLog();
            log.TryAdd(Constants.KEY_MESSAGE, mensagem);
            log.TryAdd(Constants.KEY_SEVERITY, ConvertLogLevelToString(LogLevel.Warning));
            LogLevelAccessor.LogLevel = LogLevel.Warning;
            logger.LogWarning(JsonConvert.SerializeObject(log));
        }
        public void Atencao(ExpandoObject log)
        {
            if (log is null)
                return;
            log.TryAdd(Constants.KEY_SEVERITY, ConvertLogLevelToString(LogLevel.Warning));
            LogLevelAccessor.LogLevel = LogLevel.Warning;
            logger.LogWarning(JsonConvert.SerializeObject(log, SERIALIZER_SETTINGS));
        }
        public void Erro(string mensagem)
        {
            if (string.IsNullOrWhiteSpace(mensagem))
                return;
            var log = CriarDicionarioDeLog();
            log.TryAdd(Constants.KEY_MESSAGE, mensagem);
            log.TryAdd(Constants.KEY_SEVERITY, ConvertLogLevelToString(LogLevel.Error));
            LogLevelAccessor.LogLevel = LogLevel.Error;
            logger.LogError(JsonConvert.SerializeObject(log));
        }
        public void Erro(ExpandoObject log)
        {
            if (log is null)
                return;
            log.TryAdd(Constants.KEY_SEVERITY, ConvertLogLevelToString(LogLevel.Error));
            LogLevelAccessor.LogLevel = LogLevel.Error;
            logger.LogError(JsonConvert.SerializeObject(log, SERIALIZER_SETTINGS));
        }
        public void Erro(Exception excecao)
        {
            if (excecao is null)
                return;
            var log = CriarDicionarioDeLog();
            log.TryAdd(Constants.KEY_ERROR_MESSAGE, excecao.Message);
            log.TryAdd(Constants.KEY_ERROR_TYPE, excecao.GetType().Name);
            log.TryAdd(Constants.KEY_STACK_TRACE, excecao.StackTrace);
            log.TryAdd(Constants.KEY_SEVERITY, ConvertLogLevelToString(LogLevel.Error));
            LogLevelAccessor.LogLevel = LogLevel.Error;
            logger.LogError(JsonConvert.SerializeObject(log, SERIALIZER_SETTINGS));
        }
        public void Erro(string mensagem, Exception excecao)
        {
            if (excecao is null && string.IsNullOrWhiteSpace(mensagem))
                return;
            if (string.IsNullOrWhiteSpace(mensagem))
                Erro(excecao);
            else if (excecao is null)
                Erro(mensagem);
            else
            {
                var log = CriarDicionarioDeLog();
                log.TryAdd(Constants.KEY_MESSAGE, mensagem);
                log.TryAdd(Constants.KEY_ERROR_MESSAGE, excecao.Message);
                log.TryAdd(Constants.KEY_ERROR_TYPE, excecao.GetType().Name);
                log.TryAdd(Constants.KEY_STACK_TRACE, excecao.StackTrace);
                log.TryAdd(Constants.KEY_SEVERITY, ConvertLogLevelToString(LogLevel.Error));
                LogLevelAccessor.LogLevel = LogLevel.Error;
                logger.LogError(JsonConvert.SerializeObject(log, SERIALIZER_SETTINGS));
            }
        }

        private static string ConvertLogLevelToString(LogLevel logLevel)
            => logLevel switch
            {
                LogLevel.Trace => Constants.SEVERIDADE_TRACE_STRING,
                LogLevel.Debug => Constants.SEVERIDADE_DEBUG_STRING,
                LogLevel.Information => Constants.SEVERIDADE_INFORMATION_STRING,
                LogLevel.Warning => Constants.SEVERIDADE_WARNING_STRING,
                LogLevel.Error => Constants.SEVERIDADE_ERROR_STRING,
                LogLevel.Critical => Constants.SEVERIDADE_CRITICAL_STRING,
                _ => Constants.SEVERIDADE_DEFAULT_STRING
            };

        public sealed class LoggerBuilder : ILoggerBuilder
        {
            private readonly IApiLogger<TCategoryName> logger;
            private readonly ExpandoObject dicionarioDeLog;
            private readonly Lazy<IDictionary<string, Stopwatch>> cronometros
                = new Lazy<IDictionary<string, Stopwatch>>(() => new Dictionary<string, Stopwatch>());

            private bool disposed = false;

            internal LoggerBuilder(IApiLogger<TCategoryName> logger, ExpandoObject dicionarioDeLog, KeyValuePair<string, Stopwatch> stopwatch = default)
            {
                this.logger = logger;
                this.dicionarioDeLog = dicionarioDeLog;
                if (!string.IsNullOrEmpty(stopwatch.Key))
                    AdicionarCronometro(stopwatch.Key, stopwatch.Value);
            }

            public ILoggerBuilder ComPropriedade<T>(string key, [NotNull] T value)
                where T : notnull
            {
                ValidateDisposedObject();
                if (!string.IsNullOrEmpty(key))
                {
                    if (value is not Stopwatch sw)
                        dicionarioDeLog.TryAdd(key, value);
                    else
                        AdicionarCronometro(key, sw);
                }
                return this;
            }

            public bool Rastreio(string mensagem)
            {
                ValidateDisposedObject();
                if (string.IsNullOrWhiteSpace(mensagem))
                    return false;
                dicionarioDeLog.TryAdd(Constants.KEY_MESSAGE, mensagem);
                FinalizarCronometros();
                logger.Rastreio(dicionarioDeLog);
                Dispose();
                return true;
            }
            public bool Depuracao(string mensagem)
            {
                ValidateDisposedObject();
                if (string.IsNullOrWhiteSpace(mensagem))
                    return false;
                dicionarioDeLog.TryAdd(Constants.KEY_MESSAGE, mensagem);
                FinalizarCronometros();
                logger.Depuracao(dicionarioDeLog);
                Dispose();
                return true;
            }
            public bool Informacao(string mensagem)
            {
                ValidateDisposedObject();
                if (string.IsNullOrWhiteSpace(mensagem))
                    return false;
                dicionarioDeLog.TryAdd(Constants.KEY_MESSAGE, mensagem);
                FinalizarCronometros();
                logger.Informacao(dicionarioDeLog);
                Dispose();
                return true;
            }
            public bool Atencao(string mensagem)
            {
                ValidateDisposedObject();
                if (string.IsNullOrWhiteSpace(mensagem))
                    return false;
                dicionarioDeLog.TryAdd(Constants.KEY_MESSAGE, mensagem);
                FinalizarCronometros();
                logger.Atencao(dicionarioDeLog);
                Dispose();
                return true;
            }
            public bool Erro(string mensagem)
            {
                ValidateDisposedObject();
                if (string.IsNullOrWhiteSpace(mensagem))
                    return false;
                dicionarioDeLog.TryAdd(Constants.KEY_MESSAGE, mensagem);
                FinalizarCronometros();
                logger.Erro(dicionarioDeLog);
                Dispose();
                return true;
            }
            public bool Erro(Exception excecao)
            {
                ValidateDisposedObject();
                if (excecao is null)
                    return false;
                dicionarioDeLog.TryAdd(Constants.KEY_ERROR_MESSAGE, excecao.Message);
                dicionarioDeLog.TryAdd(Constants.KEY_ERROR_TYPE, excecao.GetType().Name);
                dicionarioDeLog.TryAdd(Constants.KEY_STACK_TRACE, excecao.StackTrace);
                dicionarioDeLog.TryAdd(Constants.KEY_SEVERITY, ConvertLogLevelToString(LogLevel.Error));
                FinalizarCronometros();
                logger.Erro(dicionarioDeLog);
                Dispose();
                return true;
            }
            public bool Erro(string mensagem, Exception excecao)
            {
                ValidateDisposedObject();
                if (string.IsNullOrWhiteSpace(mensagem) && excecao is null)
                    return false;
                if (!string.IsNullOrWhiteSpace(mensagem))
                {
                    dicionarioDeLog.TryAdd(Constants.KEY_MESSAGE, mensagem);
                }
                if (excecao is not null)
                {
                    dicionarioDeLog.TryAdd(Constants.KEY_ERROR_MESSAGE, excecao.Message);
                    dicionarioDeLog.TryAdd(Constants.KEY_ERROR_TYPE, excecao.GetType().Name);
                    dicionarioDeLog.TryAdd(Constants.KEY_STACK_TRACE, excecao.StackTrace);
                    dicionarioDeLog.TryAdd(Constants.KEY_SEVERITY, ConvertLogLevelToString(LogLevel.Error));
                }
                FinalizarCronometros();
                logger.Erro(dicionarioDeLog);
                Dispose();
                return true;
            }

            private void AdicionarCronometro(string key, Stopwatch value)
            {
                if (!value.IsRunning)
                    value.Start();
                cronometros.Value.TryAdd(key, value);
            }

            private void FinalizarCronometros()
            {
                if (cronometros.IsValueCreated)
                {
                    foreach (var (key, sw) in cronometros.Value)
                    {
                        if (sw.IsRunning)
                            sw.Stop();
                        dicionarioDeLog.TryAdd(key, sw.ElapsedMilliseconds);
                    }
                }
            }

            private void ValidateDisposedObject()
            {
                if (disposed)
                    throw new ObjectDisposedException("The logger builder has disposed");
            }

            public void Dispose()
            {
                disposed = true;
                GC.SuppressFinalize(this);
            }
        }
    }

    public abstract class ApiLoggerBase
    {
        protected ApiLoggerBase() { }

        protected static readonly JsonSerializerSettings SERIALIZER_SETTINGS = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            ContractResolver = LoggerJsonContractResolver.Instance
        };
    }
}

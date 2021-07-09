using DeepEqual.Syntax;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Threading;
using System.Threading.Tasks;
using GuimoSoft.Logger.Tests.Fake;
using GuimoSoft.Logger.Utils;
using GuimoSoft.Providers.Interfaces;
using Xunit;

namespace GuimoSoft.Logger.Tests
{
    public class APILoggerTests
    {
        private static readonly JsonSerializerSettings SERIALIZER_SETTINGS = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            ContractResolver = LoggerJsonContractResolver.Instance
        };

        private readonly string tenant = "casasbahia";
        private readonly string correlationId = Guid.NewGuid().ToString("N");

        private (Mock<ILogger<APILoggerTests>>, ApiLogger<APILoggerTests>) Createinstance()
        {
            var loggerMock = new Mock<ILogger<APILoggerTests>>();
            var scopeOriginProvider = new Mock<IScopeOriginProvider>();
            scopeOriginProvider.Setup(x => x.ScopeOrigin).Returns(ScopeOrigin.Kafka);

            return (loggerMock, new ApiLogger<APILoggerTests>(loggerMock.Object, correlationId, tenant, scopeOriginProvider.Object));
        }

        [Fact]
        public void Se_CriarExpandoObject_Entao_ConteraCorrelationIdETenant()

        {
            (var loggerMock, var apiLogger) = Createinstance();

            var dicionarioDeLog = apiLogger.CriarDicionarioDeLog();
            Assert.Contains(dicionarioDeLog, kvp => kvp.Key.Equals(Constants.KEY_CORRELATION_ID) && kvp.Value.Equals(correlationId.ToString()));
            Assert.Contains(dicionarioDeLog, kvp => kvp.Key.Equals(Constants.KEY_TENANT) && kvp.Value.Equals(tenant.ToString()));
            Assert.Contains(dicionarioDeLog, kvp => kvp.Key.Equals(Constants.KEY_ESCOPO_ORIGEM) && kvp.Value.Equals(ScopeOrigin.Kafka.ToString()));
        }

        [Fact]
        public void Se_RastreioComMensagemVaziaNulaOuComEspacoEmBranco_Entao_NaoEscreveLog()
        {
            (var loggerMock, var apiLogger) = Createinstance();

            apiLogger.Rastreio(string.Empty);

            loggerMock.Verify(
                l => l.Log(
                    LogLevel.Trace,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
                    ),
                Times.Never
                );

            apiLogger.ComPropriedade(string.Empty, string.Empty).Rastreio(string.Empty);

            loggerMock.Verify(
                l => l.Log(
                    LogLevel.Trace,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
                    ),
                Times.Never
                );

            apiLogger.Rastreio("    ");

            loggerMock.Verify(
                l => l.Log(
                    LogLevel.Trace,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
                    ),
                Times.Never
                );

            apiLogger.ComPropriedade(string.Empty, string.Empty).Rastreio("    ");

            loggerMock.Verify(
                l => l.Log(
                    LogLevel.Trace,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
                    ),
                Times.Never
                );

            apiLogger.Rastreio(null as string);

            loggerMock.Verify(
                l => l.Log(
                    LogLevel.Trace,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
                    ),
                Times.Never
                );

            apiLogger.ComPropriedade(string.Empty, string.Empty).Rastreio(null as string);

            loggerMock.Verify(
                l => l.Log(
                    LogLevel.Trace,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
                    ),
                Times.Never
                );
        }

        [Fact]
        public void Se_RastreioExpandoNuloPreenchida_Entao_EscreveLog()
        {
            (var loggerMock, var apiLogger) = Createinstance();

            apiLogger.Rastreio(null as ExpandoObject);

            loggerMock.Verify(
                l => l.Log(
                    LogLevel.Trace,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
                    ),
                Times.Never
                );
        }

        [Fact]
        public void Se_RastreioComMensagemPreenchida_Entao_EscreveLog()
        {
            const string errorMessage = "Warning";
            (var loggerMock, var apiLogger) = Createinstance();

            var expectedExpando = apiLogger.CriarDicionarioDeLog();
            expectedExpando.TryAdd(Constants.KEY_MESSAGE, errorMessage);
            expectedExpando.TryAdd(Constants.KEY_SEVERITY, Constants.SEVERIDADE_TRACE_STRING);

            var expected = JsonConvert.SerializeObject(expectedExpando, SERIALIZER_SETTINGS);

            apiLogger.Rastreio(errorMessage);

            loggerMock.Verify(
                l => l.Log(
                    LogLevel.Trace,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Equals(expected)),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
                    ),
                Times.Once
                );

            apiLogger.ComPropriedade(string.Empty, string.Empty).Rastreio(errorMessage);

            loggerMock.Verify(
                l => l.Log(
                    LogLevel.Trace,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Equals(expected)),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
                    ),
                Times.Exactly(2)
                );
        }

        [Fact]
        public void Se_RastreioExpandoPreenchida_Entao_EscreveLog()
        {
            const string errorMessage = "Ocorreu um erro";
            var errorException = new Exception(errorMessage);
            (var loggerMock, var apiLogger) = Createinstance();

            var expectedExpando = apiLogger.CriarDicionarioDeLog();
            expectedExpando.TryAdd(Constants.KEY_MESSAGE, errorMessage);
            expectedExpando.TryAdd("key-0", "teste");
            expectedExpando.TryAdd(Constants.KEY_SEVERITY, Constants.SEVERIDADE_TRACE_STRING);

            var expected = JsonConvert.SerializeObject(expectedExpando, SERIALIZER_SETTINGS);

            var dicionarioDeLog = apiLogger.CriarDicionarioDeLog();

            dicionarioDeLog.TryAdd(Constants.KEY_MESSAGE, errorMessage);
            dicionarioDeLog.TryAdd("key-0", "teste");

            apiLogger.Rastreio(dicionarioDeLog);

            loggerMock.Verify(
                l => l.Log(
                    LogLevel.Trace,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Equals(expected)),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
                    ),
                Times.Once
                );

            apiLogger.ComPropriedade(string.Empty, string.Empty)
                .ComPropriedade(Constants.KEY_MESSAGE, errorMessage)
                .ComPropriedade("key-0", "teste")
                .Rastreio(errorMessage);

            loggerMock.Verify(
                l => l.Log(
                    LogLevel.Trace,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Equals(expected)),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
                    ),
                Times.Exactly(2)
                );
        }

        [Fact]
        public void Se_DepuracaoComMensagemVaziaNulaOuComEspacoEmBranco_Entao_NaoEscreveLog()
        {
            (var loggerMock, var apiLogger) = Createinstance();

            apiLogger.Depuracao(string.Empty);

            loggerMock.Verify(
                l => l.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
                    ),
                Times.Never
                );

            apiLogger.ComPropriedade(string.Empty, string.Empty).Depuracao(string.Empty);

            loggerMock.Verify(
                l => l.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
                    ),
                Times.Never
                );

            apiLogger.Depuracao("    ");

            loggerMock.Verify(
                l => l.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
                    ),
                Times.Never
                );

            apiLogger.ComPropriedade(string.Empty, string.Empty).Depuracao("    ");

            loggerMock.Verify(
                l => l.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
                    ),
                Times.Never
                );

            apiLogger.Depuracao(null as string);

            loggerMock.Verify(
                l => l.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
                    ),
                Times.Never
                );

            apiLogger.ComPropriedade(string.Empty, string.Empty).Depuracao(null as string);

            loggerMock.Verify(
                l => l.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
                    ),
                Times.Never
                );
        }

        [Fact]
        public void Se_DepuracaoExpandoNuloPreenchida_Entao_EscreveLog()
        {
            (var loggerMock, var apiLogger) = Createinstance();

            apiLogger.Depuracao(null as ExpandoObject);

            loggerMock.Verify(
                l => l.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
                    ),
                Times.Never
                );
        }

        [Fact]
        public void Se_DepuracaoComMensagemPreenchida_Entao_EscreveLog()
        {
            const string errorMessage = "Warning";
            (var loggerMock, var apiLogger) = Createinstance();

            var expectedExpando = apiLogger.CriarDicionarioDeLog();
            expectedExpando.TryAdd(Constants.KEY_MESSAGE, errorMessage);
            expectedExpando.TryAdd(Constants.KEY_SEVERITY, Constants.SEVERIDADE_DEBUG_STRING);

            var expected = JsonConvert.SerializeObject(expectedExpando, SERIALIZER_SETTINGS);

            apiLogger.Depuracao(errorMessage);

            loggerMock.Verify(
                l => l.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Equals(expected)),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
                    ),
                Times.Once
                );

            apiLogger.ComPropriedade(string.Empty, string.Empty).Depuracao(errorMessage);

            loggerMock.Verify(
                l => l.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Equals(expected)),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
                    ),
                Times.Exactly(2)
                );
        }

        [Fact]
        public void Se_DepuracaoExpandoPreenchida_Entao_EscreveLog()
        {
            const string errorMessage = "Ocorreu um erro";
            var errorException = new Exception(errorMessage);
            (var loggerMock, var apiLogger) = Createinstance();

            var expectedExpando = apiLogger.CriarDicionarioDeLog();
            expectedExpando.TryAdd(Constants.KEY_MESSAGE, errorMessage);
            expectedExpando.TryAdd("key-0", "teste");
            expectedExpando.TryAdd(Constants.KEY_SEVERITY, Constants.SEVERIDADE_DEBUG_STRING);

            var expected = JsonConvert.SerializeObject(expectedExpando, SERIALIZER_SETTINGS);

            var dicionarioDeLog = apiLogger.CriarDicionarioDeLog();

            dicionarioDeLog.TryAdd(Constants.KEY_MESSAGE, errorMessage);
            dicionarioDeLog.TryAdd("key-0", "teste");

            apiLogger.Depuracao(dicionarioDeLog);

            loggerMock.Verify(
                l => l.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Equals(expected)),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
                    ),
                Times.Once
                );

            apiLogger.ComPropriedade(string.Empty, string.Empty)
                .ComPropriedade(Constants.KEY_MESSAGE, errorMessage)
                .ComPropriedade("key-0", "teste")
                .Depuracao(errorMessage);

            loggerMock.Verify(
                l => l.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Equals(expected)),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
                    ),
                Times.Exactly(2)
                );
        }

        [Fact]
        public void Se_InformacaoComMensagemVaziaNulaOuComEspacoEmBranco_Entao_NaoEscreveLog()
        {
            (var loggerMock, var apiLogger) = Createinstance();

            apiLogger.Informacao(string.Empty);

            loggerMock.Verify(
                l => l.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
                    ),
                Times.Never
                );

            apiLogger.ComPropriedade(string.Empty, string.Empty).Informacao(string.Empty);

            loggerMock.Verify(
                l => l.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
                    ),
                Times.Never
                );

            apiLogger.Informacao("    ");

            loggerMock.Verify(
                l => l.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
                    ),
                Times.Never
                );

            apiLogger.ComPropriedade(string.Empty, string.Empty).Informacao("    ");

            loggerMock.Verify(
                l => l.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
                    ),
                Times.Never
                );

            apiLogger.Informacao(null as string);

            loggerMock.Verify(
                l => l.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
                    ),
                Times.Never
                );

            apiLogger.ComPropriedade(string.Empty, string.Empty).Informacao(null as string);

            loggerMock.Verify(
                l => l.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
                    ),
                Times.Never
                );
        }

        [Fact]
        public void Se_InformacaoExpandoNuloPreenchida_Entao_EscreveLog()
        {
            (var loggerMock, var apiLogger) = Createinstance();

            apiLogger.Informacao(null as ExpandoObject);

            loggerMock.Verify(
                l => l.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
                    ),
                Times.Never
                );
        }

        [Fact]
        public void Se_InformacaoComMensagemPreenchida_Entao_EscreveLog()
        {
            const string errorMessage = "Warning";
            (var loggerMock, var apiLogger) = Createinstance();

            var expectedExpando = apiLogger.CriarDicionarioDeLog();
            expectedExpando.TryAdd(Constants.KEY_MESSAGE, errorMessage);
            expectedExpando.TryAdd(Constants.KEY_SEVERITY, Constants.SEVERIDADE_INFORMATION_STRING);

            var expected = JsonConvert.SerializeObject(expectedExpando, SERIALIZER_SETTINGS);

            apiLogger.Informacao(errorMessage);

            loggerMock.Verify(
                l => l.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Equals(expected)),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
                    ),
                Times.Once
                );

            apiLogger.ComPropriedade(string.Empty, string.Empty).Informacao(errorMessage);

            loggerMock.Verify(
                l => l.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Equals(expected)),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
                    ),
                Times.Exactly(2)
                );
        }

        [Fact]
        public void Se_InformacaoExpandoPreenchida_Entao_EscreveLog()
        {
            const string errorMessage = "Ocorreu um erro";
            var errorException = new Exception(errorMessage);
            (var loggerMock, var apiLogger) = Createinstance();

            var expectedExpando = apiLogger.CriarDicionarioDeLog();
            expectedExpando.TryAdd(Constants.KEY_MESSAGE, errorMessage);
            expectedExpando.TryAdd("key-0", "teste");
            expectedExpando.TryAdd(Constants.KEY_SEVERITY, Constants.SEVERIDADE_INFORMATION_STRING);

            var expected = JsonConvert.SerializeObject(expectedExpando, SERIALIZER_SETTINGS);

            var dicionarioDeLog = apiLogger.CriarDicionarioDeLog();

            dicionarioDeLog.TryAdd(Constants.KEY_MESSAGE, errorMessage);
            dicionarioDeLog.TryAdd("key-0", "teste");

            apiLogger.Informacao(dicionarioDeLog);

            loggerMock.Verify(
                l => l.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Equals(expected)),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
                    ),
                Times.Once
                );

            apiLogger.ComPropriedade(string.Empty, string.Empty)
                .ComPropriedade(Constants.KEY_MESSAGE, errorMessage)
                .ComPropriedade("key-0", "teste")
                .Informacao(errorMessage);

            loggerMock.Verify(
                l => l.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Equals(expected)),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
                    ),
                Times.Exactly(2)
                );
        }

        [Fact]
        public void Se_WarningComMensagemVaziaNulaOuComEspacoEmBranco_Entao_NaoEscreveLog()
        {
            (var loggerMock, var apiLogger) = Createinstance();

            apiLogger.Atencao(string.Empty);

            loggerMock.Verify(
                l => l.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
                    ),
                Times.Never
                );

            apiLogger.ComPropriedade(string.Empty, string.Empty).Atencao(string.Empty);

            loggerMock.Verify(
                l => l.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
                    ),
                Times.Never
                );

            apiLogger.Atencao("    ");

            loggerMock.Verify(
                l => l.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
                    ),
                Times.Never
                );

            apiLogger.ComPropriedade(string.Empty, string.Empty).Atencao("    ");

            loggerMock.Verify(
                l => l.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
                    ),
                Times.Never
                );

            apiLogger.Atencao(null as string);

            loggerMock.Verify(
                l => l.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
                    ),
                Times.Never
                );

            apiLogger.ComPropriedade(string.Empty, string.Empty).Atencao(null as string);

            loggerMock.Verify(
                l => l.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
                    ),
                Times.Never
                );
        }

        [Fact]
        public void Se_WarningExpandoNuloPreenchida_Entao_EscreveLog()
        {
            (var loggerMock, var apiLogger) = Createinstance();

            apiLogger.Atencao(null as ExpandoObject);

            loggerMock.Verify(
                l => l.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
                    ),
                Times.Never
                );
        }

        [Fact]
        public void Se_WarningComMensagemPreenchida_Entao_EscreveLog()
        {
            const string errorMessage = "Warning";
            (var loggerMock, var apiLogger) = Createinstance();

            var expectedExpando = apiLogger.CriarDicionarioDeLog();
            expectedExpando.TryAdd(Constants.KEY_MESSAGE, errorMessage);
            expectedExpando.TryAdd(Constants.KEY_SEVERITY, Constants.SEVERIDADE_WARNING_STRING);

            var expected = JsonConvert.SerializeObject(expectedExpando, SERIALIZER_SETTINGS);

            apiLogger.Atencao(errorMessage);

            loggerMock.Verify(
                l => l.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Equals(expected)),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
                    ),
                Times.Once
                );

            apiLogger.ComPropriedade(string.Empty, string.Empty).Atencao(errorMessage);

            loggerMock.Verify(
                l => l.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Equals(expected)),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
                    ),
                Times.Exactly(2)
                );
        }

        [Fact]
        public void Se_WarningExpandoPreenchida_Entao_EscreveLog()
        {
            const string errorMessage = "Ocorreu um erro";
            var errorException = new Exception(errorMessage);
            (var loggerMock, var apiLogger) = Createinstance();

            var expectedExpando = apiLogger.CriarDicionarioDeLog();
            expectedExpando.TryAdd(Constants.KEY_MESSAGE, errorMessage);
            expectedExpando.TryAdd("key-0", "teste");
            expectedExpando.TryAdd(Constants.KEY_SEVERITY, Constants.SEVERIDADE_WARNING_STRING);


            var expected = JsonConvert.SerializeObject(expectedExpando, SERIALIZER_SETTINGS);

            var dicionarioDeLog = apiLogger.CriarDicionarioDeLog();

            dicionarioDeLog.TryAdd(Constants.KEY_MESSAGE, errorMessage);
            dicionarioDeLog.TryAdd("key-0", "teste");
            dicionarioDeLog.TryAdd("key-0", "teste");

            apiLogger.Atencao(dicionarioDeLog);

            loggerMock.Verify(
                l => l.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Equals(expected)),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
                    ),
                Times.Once
                );

            apiLogger.ComPropriedade(string.Empty, string.Empty)
                .ComPropriedade(Constants.KEY_MESSAGE, errorMessage)
                .ComPropriedade("key-0", "teste")
                .Atencao(errorMessage);

            loggerMock.Verify(
                l => l.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Equals(expected)),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
                    ),
                Times.Exactly(2)
                );
        }

        [Fact]
        public void Se_ErroComMensagemVaziaNulaOuComEspacoEmBranco_Entao_NaoEscreveLog()
        {
            (var loggerMock, var apiLogger) = Createinstance();

            apiLogger.Erro(string.Empty);

            loggerMock.Verify(
                l => l.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
                    ),
                Times.Never
                );

            apiLogger.ComPropriedade(string.Empty, string.Empty).Erro(string.Empty);

            loggerMock.Verify(
                l => l.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
                    ),
                Times.Never
                );

            apiLogger.Erro("    ");

            loggerMock.Verify(
                l => l.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
                    ),
                Times.Never
                );

            apiLogger.ComPropriedade(string.Empty, string.Empty).Erro("    ");

            loggerMock.Verify(
                l => l.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
                    ),
                Times.Never
                );

            apiLogger.Erro(null as string);

            loggerMock.Verify(
                l => l.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
                    ),
                Times.Never
                );

            apiLogger.ComPropriedade(string.Empty, string.Empty).Erro(null as string);

            loggerMock.Verify(
                l => l.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
                    ),
                Times.Never
                );
        }

        [Fact]
        public void Se_ErroExceptionNulaPreenchida_Entao_NaoEscreveLog()
        {
            (var loggerMock, var apiLogger) = Createinstance();

            apiLogger.Erro(null as Exception);

            loggerMock.Verify(
                l => l.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
                    ),
                Times.Never
                );

            apiLogger
                .ComPropriedade(string.Empty, string.Empty)
                .Erro(null as Exception)
                .Should().BeFalse();

            loggerMock.Verify(
                l => l.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
                    ),
                Times.Never
                );
        }

        [Fact]
        public void Se_ErroExceptionNulaEMensagemVaziaNulaOuComEspacoEmBranco_Entao_NaoEscreveLog()
        {
            (var loggerMock, var apiLogger) = Createinstance();

            apiLogger.Erro(string.Empty, null as Exception);

            loggerMock.Verify(
                l => l.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
                    ),
                Times.Never
                );

            apiLogger.ComPropriedade(string.Empty, string.Empty).Erro(string.Empty, null as Exception);

            loggerMock.Verify(
                l => l.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
                    ),
                Times.Never
                );

            apiLogger.Erro("    ", null as Exception);

            loggerMock.Verify(
                l => l.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
                    ),
                Times.Never
                );

            apiLogger.ComPropriedade(string.Empty, string.Empty).Erro("    ", null as Exception);

            loggerMock.Verify(
                l => l.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
                    ),
                Times.Never
                );

            apiLogger.Erro(null as string, null as Exception);

            loggerMock.Verify(
                l => l.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
                    ),
                Times.Never
                );

            apiLogger.ComPropriedade(string.Empty, string.Empty).Erro(null as string, null as Exception);

            loggerMock.Verify(
                l => l.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
                    ),
                Times.Never
                );
        }

        [Fact]
        public void Se_ErroComMensagemPreenchida_Entao_EscreveLog()
        {
            const string errorMessage = "Error";
            (var loggerMock, var apiLogger) = Createinstance();

            var expectedExpando = apiLogger.CriarDicionarioDeLog();
            expectedExpando.TryAdd(Constants.KEY_MESSAGE, errorMessage);
            expectedExpando.TryAdd(Constants.KEY_SEVERITY, Constants.SEVERIDADE_ERROR_STRING);

            var expected = JsonConvert.SerializeObject(expectedExpando, SERIALIZER_SETTINGS);

            apiLogger.Erro(errorMessage);

            loggerMock.Verify(
                l => l.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Equals(expected)),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
                    ),
                Times.Once
                );

            apiLogger
                .ComPropriedade(string.Empty, string.Empty)
                .Erro(errorMessage)
                .Should().BeTrue();

            loggerMock.Verify(
                l => l.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Equals(expected)),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
                    ),
                Times.Exactly(2)
                );
        }

        [Fact]
        public void Se_ErroExceptionPreenchida_Entao_EscreveLog()
        {
            const string errorMessage = "Ocorreu um erro";
            var exception = new Exception(errorMessage);
            (var loggerMock, var apiLogger) = Createinstance();

            var expectedExpando = apiLogger.CriarDicionarioDeLog();
            expectedExpando.TryAdd(Constants.KEY_ERROR_MESSAGE, exception.Message);
            expectedExpando.TryAdd(Constants.KEY_ERROR_TYPE, exception.GetType().Name);
            expectedExpando.TryAdd(Constants.KEY_STACK_TRACE, exception.StackTrace);
            expectedExpando.TryAdd(Constants.KEY_SEVERITY, Constants.SEVERIDADE_ERROR_STRING);

            var expected = JsonConvert.SerializeObject(expectedExpando, SERIALIZER_SETTINGS);

            apiLogger.Erro(exception);

            loggerMock.Verify(
                l => l.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => AreSameProperties(v, expected)),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
                    ),
                Times.Once
                );

            apiLogger.ComPropriedade(string.Empty, string.Empty)
                .Erro(exception);

            loggerMock.Verify(
                l => l.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => AreSameProperties(v, expected)),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
                    ),
                Times.Exactly(2)
                );
        }

        [Fact]
        public void Se_ErroComMensagemEExceptionPreenchida_Entao_EscreveLog()
        {
            const string errorMessage = "Ocorreu um erro";
            var exception = new Exception(errorMessage);
            (var loggerMock, var apiLogger) = Createinstance();

            var expectedExpando = apiLogger.CriarDicionarioDeLog();
            expectedExpando.TryAdd(Constants.KEY_MESSAGE, errorMessage);
            expectedExpando.TryAdd(Constants.KEY_ERROR_MESSAGE, exception.Message);
            expectedExpando.TryAdd(Constants.KEY_ERROR_TYPE, exception.GetType().Name);
            expectedExpando.TryAdd(Constants.KEY_STACK_TRACE, exception.StackTrace);
            expectedExpando.TryAdd(Constants.KEY_SEVERITY, Constants.SEVERIDADE_ERROR_STRING);

            var expected = JsonConvert.SerializeObject(expectedExpando, SERIALIZER_SETTINGS);

            apiLogger.Erro(errorMessage, exception);

            loggerMock.Verify(
                l => l.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Equals(expected)),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
                    ),
                Times.Once
                );

            apiLogger
                .ComPropriedade(string.Empty, string.Empty)
                .Erro(errorMessage, exception)
                .Should().BeTrue();

            loggerMock.Verify(
                l => l.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Equals(expected)),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
                    ),
                Times.Exactly(2)
                );
        }

        [Fact]
        public void Dado_UmObjetoComPropriedadesParaSeremIgnoradas_Se_Logar_Entao_AsPropriedadesMarcadasSeraoExcluidasDoJson()
        {
            (var loggerMock, var apiLogger) = Createinstance();
            var expectedObject = new TesteLog();

            var expectedJson = JsonConvert.SerializeObject(expectedObject, SERIALIZER_SETTINGS);

            var expectedExpando = apiLogger.CriarDicionarioDeLog();
            expectedExpando.TryAdd("objeto", expectedObject);
            expectedExpando.TryAdd(Constants.KEY_MESSAGE, "Teste rastreio");
            expectedExpando.TryAdd(Constants.KEY_SEVERITY, Constants.SEVERIDADE_TRACE_STRING);

            var expected = JsonConvert.SerializeObject(expectedExpando, SERIALIZER_SETTINGS);

            apiLogger
                .ComPropriedade("objeto", expectedObject)
                .Rastreio("Teste rastreio");

            loggerMock.Verify(
                l => l.Log(
                    LogLevel.Trace,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => AreSameProperties(v, expected)),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
                    ),
                Times.Once
                );
        }

        [Fact]
        public async Task Dado_UmStopwatch_SeLogar_Entao_OStopwatchEhIniciadoEFinalizado()
        {
            var stopwatch = new Stopwatch();

            (_, var apiLogger) = Createinstance();

            stopwatch
                .IsRunning.Should().BeFalse();

            var logger = apiLogger
                .ComPropriedade("elapsed-time", stopwatch);

            stopwatch
                .IsRunning.Should().BeTrue();

            await Task.Delay(2);
            logger
                .Rastreio("logado");

            stopwatch
                .IsRunning.Should().BeFalse();
        }

        private bool AreSameProperties(object @object, string expected)
        {
            var deserializedExpected = JsonConvert.DeserializeObject<IDictionary<string, object>>(expected);
            var deserialized = JsonConvert.DeserializeObject<IDictionary<string, object>>(@object.ToString());
            var result = deserialized.IsDeepEqual(deserializedExpected);
            return result;
        }
    }
}

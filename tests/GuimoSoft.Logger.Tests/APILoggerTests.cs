﻿using DeepEqual.Syntax;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Threading.Tasks;
using GuimoSoft.Logger.Tests.Fake;
using GuimoSoft.Logger.Utils;
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
            var contextAccessor = new ApiLoggerContextAccessor();
            contextAccessor.Context = new Dictionary<string, object>();
            contextAccessor.Context.Add(Constants.KEY_CORRELATION_ID, correlationId.ToString());
            contextAccessor.Context.Add(Constants.KEY_TENANT, tenant.ToString());

            return (loggerMock, new ApiLogger<APILoggerTests>(loggerMock.Object, contextAccessor));
        }

        private (Mock<ILogger<APILoggerTests>>, ApiLogger<APILoggerTests>) CreateinstanceWithoutContext()
        {
            var loggerMock = new Mock<ILogger<APILoggerTests>>();

            return (loggerMock, new ApiLogger<APILoggerTests>(loggerMock.Object, null));
        }

        [Fact]
        public void Se_CriarExpandoObject_Entao_ConteraCorrelationIdETenant()

        {
            (var loggerMock, var apiLogger) = Createinstance();

            var dicionarioDeLog = apiLogger.CriarDicionarioDeLog();
            Assert.Contains(dicionarioDeLog, kvp => kvp.Key.Equals(Constants.KEY_CORRELATION_ID) && kvp.Value.Equals(correlationId.ToString()));
            Assert.Contains(dicionarioDeLog, kvp => kvp.Key.Equals(Constants.KEY_TENANT) && kvp.Value.Equals(tenant.ToString()));
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
            const string errorEvent = "Warning";
            (var loggerMock, var apiLogger) = Createinstance();

            var expectedExpando = apiLogger.CriarDicionarioDeLog();
            expectedExpando.TryAdd(Constants.KEY_MESSAGE, errorEvent);
            expectedExpando.TryAdd(Constants.KEY_SEVERITY, Constants.SEVERIDADE_TRACE_STRING);

            var expected = JsonConvert.SerializeObject(expectedExpando, SERIALIZER_SETTINGS);

            apiLogger.Rastreio(errorEvent);

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

            apiLogger.ComPropriedade(string.Empty, string.Empty).Rastreio(errorEvent);

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
            const string errorEvent = "Ocorreu um erro";
            var errorException = new Exception(errorEvent);
            (var loggerMock, var apiLogger) = Createinstance();

            var expectedExpando = apiLogger.CriarDicionarioDeLog();
            expectedExpando.TryAdd(Constants.KEY_MESSAGE, errorEvent);
            expectedExpando.TryAdd("key-0", "teste");
            expectedExpando.TryAdd(Constants.KEY_SEVERITY, Constants.SEVERIDADE_TRACE_STRING);

            var expected = JsonConvert.SerializeObject(expectedExpando, SERIALIZER_SETTINGS);

            var dicionarioDeLog = apiLogger.CriarDicionarioDeLog();

            dicionarioDeLog.TryAdd(Constants.KEY_MESSAGE, errorEvent);
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
                .ComPropriedade(Constants.KEY_MESSAGE, errorEvent)
                .ComPropriedade("key-0", "teste")
                .Rastreio(errorEvent);

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
            const string errorEvent = "Warning";
            (var loggerMock, var apiLogger) = Createinstance();

            var expectedExpando = apiLogger.CriarDicionarioDeLog();
            expectedExpando.TryAdd(Constants.KEY_MESSAGE, errorEvent);
            expectedExpando.TryAdd(Constants.KEY_SEVERITY, Constants.SEVERIDADE_DEBUG_STRING);

            var expected = JsonConvert.SerializeObject(expectedExpando, SERIALIZER_SETTINGS);

            apiLogger.Depuracao(errorEvent);

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

            apiLogger.ComPropriedade(string.Empty, string.Empty).Depuracao(errorEvent);

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
            const string errorEvent = "Ocorreu um erro";
            var errorException = new Exception(errorEvent);
            (var loggerMock, var apiLogger) = Createinstance();

            var expectedExpando = apiLogger.CriarDicionarioDeLog();
            expectedExpando.TryAdd(Constants.KEY_MESSAGE, errorEvent);
            expectedExpando.TryAdd("key-0", "teste");
            expectedExpando.TryAdd(Constants.KEY_SEVERITY, Constants.SEVERIDADE_DEBUG_STRING);

            var expected = JsonConvert.SerializeObject(expectedExpando, SERIALIZER_SETTINGS);

            var dicionarioDeLog = apiLogger.CriarDicionarioDeLog();

            dicionarioDeLog.TryAdd(Constants.KEY_MESSAGE, errorEvent);
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
                .ComPropriedade(Constants.KEY_MESSAGE, errorEvent)
                .ComPropriedade("key-0", "teste")
                .Depuracao(errorEvent);

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
            const string errorEvent = "Warning";
            (var loggerMock, var apiLogger) = Createinstance();

            var expectedExpando = apiLogger.CriarDicionarioDeLog();
            expectedExpando.TryAdd(Constants.KEY_MESSAGE, errorEvent);
            expectedExpando.TryAdd(Constants.KEY_SEVERITY, Constants.SEVERIDADE_INFORMATION_STRING);

            var expected = JsonConvert.SerializeObject(expectedExpando, SERIALIZER_SETTINGS);

            apiLogger.Informacao(errorEvent);

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

            apiLogger.ComPropriedade(string.Empty, string.Empty).Informacao(errorEvent);

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
            const string errorEvent = "Ocorreu um erro";
            var errorException = new Exception(errorEvent);
            (var loggerMock, var apiLogger) = Createinstance();

            var expectedExpando = apiLogger.CriarDicionarioDeLog();
            expectedExpando.TryAdd(Constants.KEY_MESSAGE, errorEvent);
            expectedExpando.TryAdd("key-0", "teste");
            expectedExpando.TryAdd(Constants.KEY_SEVERITY, Constants.SEVERIDADE_INFORMATION_STRING);

            var expected = JsonConvert.SerializeObject(expectedExpando, SERIALIZER_SETTINGS);

            var dicionarioDeLog = apiLogger.CriarDicionarioDeLog();

            dicionarioDeLog.TryAdd(Constants.KEY_MESSAGE, errorEvent);
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
                .ComPropriedade(Constants.KEY_MESSAGE, errorEvent)
                .ComPropriedade("key-0", "teste")
                .Informacao(errorEvent);

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
            const string errorEvent = "Warning";
            (var loggerMock, var apiLogger) = Createinstance();

            var expectedExpando = apiLogger.CriarDicionarioDeLog();
            expectedExpando.TryAdd(Constants.KEY_MESSAGE, errorEvent);
            expectedExpando.TryAdd(Constants.KEY_SEVERITY, Constants.SEVERIDADE_WARNING_STRING);

            var expected = JsonConvert.SerializeObject(expectedExpando, SERIALIZER_SETTINGS);

            apiLogger.Atencao(errorEvent);

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

            apiLogger.ComPropriedade(string.Empty, string.Empty).Atencao(errorEvent);

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
            const string errorEvent = "Ocorreu um erro";
            var errorException = new Exception(errorEvent);
            (var loggerMock, var apiLogger) = Createinstance();

            var expectedExpando = apiLogger.CriarDicionarioDeLog();
            expectedExpando.TryAdd(Constants.KEY_MESSAGE, errorEvent);
            expectedExpando.TryAdd("key-0", "teste");
            expectedExpando.TryAdd(Constants.KEY_SEVERITY, Constants.SEVERIDADE_WARNING_STRING);


            var expected = JsonConvert.SerializeObject(expectedExpando, SERIALIZER_SETTINGS);

            var dicionarioDeLog = apiLogger.CriarDicionarioDeLog();

            dicionarioDeLog.TryAdd(Constants.KEY_MESSAGE, errorEvent);
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
                .ComPropriedade(Constants.KEY_MESSAGE, errorEvent)
                .ComPropriedade("key-0", "teste")
                .Atencao(errorEvent);

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

            apiLogger.Erro(null as ExpandoObject);

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
            const string errorEvent = "Error";
            (var loggerMock, var apiLogger) = Createinstance();

            var expectedExpando = apiLogger.CriarDicionarioDeLog();
            expectedExpando.TryAdd(Constants.KEY_MESSAGE, errorEvent);
            expectedExpando.TryAdd(Constants.KEY_SEVERITY, Constants.SEVERIDADE_ERROR_STRING);

            var expected = JsonConvert.SerializeObject(expectedExpando, SERIALIZER_SETTINGS);

            apiLogger.Erro(errorEvent);

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
                .Erro(errorEvent)
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
            const string errorEvent = "Ocorreu um erro";
            var exception = new Exception(errorEvent);
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
            const string errorEvent = "Ocorreu um erro";
            var exception = new Exception(errorEvent);
            (var loggerMock, var apiLogger) = Createinstance();

            var expectedExpando = apiLogger.CriarDicionarioDeLog();
            expectedExpando.TryAdd(Constants.KEY_MESSAGE, errorEvent);
            expectedExpando.TryAdd(Constants.KEY_ERROR_MESSAGE, exception.Message);
            expectedExpando.TryAdd(Constants.KEY_ERROR_TYPE, exception.GetType().Name);
            expectedExpando.TryAdd(Constants.KEY_STACK_TRACE, exception.StackTrace);
            expectedExpando.TryAdd(Constants.KEY_SEVERITY, Constants.SEVERIDADE_ERROR_STRING);

            var expected = JsonConvert.SerializeObject(expectedExpando, SERIALIZER_SETTINGS);

            apiLogger.Erro(errorEvent, exception);

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
                .Erro(errorEvent, exception)
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
        public void Se_ErroComMensagemPreenchidaEExceptionNula_Entao_EscreveLog()
        {
            const string errorEvent = "Ocorreu um erro";
            Exception exception = null;
            (var loggerMock, var apiLogger) = Createinstance();

            var expectedExpando = apiLogger.CriarDicionarioDeLog();
            expectedExpando.TryAdd(Constants.KEY_MESSAGE, errorEvent);
            expectedExpando.TryAdd(Constants.KEY_SEVERITY, Constants.SEVERIDADE_ERROR_STRING);

            var expected = JsonConvert.SerializeObject(expectedExpando, SERIALIZER_SETTINGS);

            apiLogger.Erro(errorEvent, exception);

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
                .Erro(errorEvent, exception)
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
        public void Se_ErroComExceptionPreenchidaEMensagemNula_Entao_EscreveLog()
        {
            const string errorEvent = null;
            var exception = new Exception(errorEvent);
            (var loggerMock, var apiLogger) = CreateinstanceWithoutContext();

            var expectedExpando = apiLogger.CriarDicionarioDeLog();
            expectedExpando.TryAdd(Constants.KEY_ERROR_MESSAGE, exception.Message);
            expectedExpando.TryAdd(Constants.KEY_ERROR_TYPE, exception.GetType().Name);
            expectedExpando.TryAdd(Constants.KEY_STACK_TRACE, exception.StackTrace);
            expectedExpando.TryAdd(Constants.KEY_SEVERITY, Constants.SEVERIDADE_ERROR_STRING);

            var expected = JsonConvert.SerializeObject(expectedExpando, SERIALIZER_SETTINGS);

            apiLogger.Erro(errorEvent, exception);

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
                .Erro(errorEvent, exception)
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

            var expectedIgnoredObject = new IgnoredTesteLog();

            var expectedIgnoredJson = JsonConvert.SerializeObject(expectedIgnoredObject, SERIALIZER_SETTINGS);

            var expectedIgnoredExpando = apiLogger.CriarDicionarioDeLog();
            expectedIgnoredExpando.TryAdd("objeto", expectedIgnoredObject);
            expectedIgnoredExpando.TryAdd(Constants.KEY_MESSAGE, "Teste rastreio");
            expectedIgnoredExpando.TryAdd(Constants.KEY_SEVERITY, Constants.SEVERIDADE_TRACE_STRING);

            var expectedIgnored = JsonConvert.SerializeObject(expectedIgnoredExpando, SERIALIZER_SETTINGS);

            apiLogger
                .ComPropriedade("objeto", expectedIgnoredObject)
                .Rastreio("Teste rastreio");

            loggerMock.Verify(
                l => l.Log(
                    LogLevel.Trace,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => AreSameProperties(v, expectedIgnored)),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
                    ),
                Times.Once
                );
        }

        [Fact]
        public async Task Dado_UmStopwatch_SeLogar_Entao_OStopwatchEhIniciadoEFinalizado()
        {
            var stopwatch1 = new Stopwatch();
            var stopwatch2 = new Stopwatch();

            (_, var apiLogger) = Createinstance();

            stopwatch1
                .IsRunning.Should().BeFalse();

            var logger = apiLogger
                .ComPropriedade("elapsed-time", stopwatch1)
                .ComPropriedade("elapsed-time-2", stopwatch2);

            stopwatch1
                .IsRunning.Should().BeTrue();
            stopwatch2
                .IsRunning.Should().BeTrue();

            await Task.Delay(2);
            logger
                .Rastreio("logado");

            Assert.Throws<ObjectDisposedException>(() => logger.Rastreio("teste"));

            stopwatch1
                .IsRunning.Should().BeFalse();
            stopwatch2
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

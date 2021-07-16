using GuimoSoft.Logger.Attributes;
using Microsoft.Extensions.Logging;

namespace GuimoSoft.Logger.Tests.Fake
{
    public class TesteLog
    {
        public int PropriedadeLogavel { get; set; } = 0;


        // Válido para dados da LGPD
        [LoggerIgnore]
        public int PropriedadeNaoLogavelEmNenhumLog { get; set; } = 1;

        [LoggerIgnore(LogLevel.Trace)]
        public int PropriedadeNaoLogavelEmRastreio { get; set; } = 2;

        [LoggerIgnore(LogLevel.Debug)]
        public int PropriedadeNaoLogavelEmDepuracao { get; set; } = 3;

        [LoggerIgnore(LogLevel.Information)]
        public int PropriedadeNaoLogavelEmInformacao { get; set; } = 4;

        [LoggerIgnore(LogLevel.Warning)]
        public int PropriedadeNaoLogavelEmAtencao { get; set; } = 5;

        [LoggerIgnore(LogLevel.Error)]
        public int PropriedadeNaoLogavelEmErro { get; set; } = 6;


        [LoggerIgnore(LogLevel.Trace, LogLevel.Debug, LogLevel.Information)]
        public int PropriedadeLogavelApenasEmAtencaoEErro { get; set; } = 7;
    }
}

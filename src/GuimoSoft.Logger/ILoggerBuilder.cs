using System;
using System.Diagnostics.CodeAnalysis;

namespace GuimoSoft.Logger
{
    public interface ILoggerBuilder : IDisposable
    {
        ILoggerBuilder ComPropriedade<T>(string key, [NotNull] T value) where T : notnull;

        bool Rastreio(string mensagem);
        bool Depuracao(string mensagem);
        bool Informacao(string mensagem);
        bool Atencao(string mensagem);
        bool Erro(string mensagem);
        bool Erro([NotNull] Exception excecao);
        bool Erro(string mensagem, [NotNull] Exception excecao);
    }
}

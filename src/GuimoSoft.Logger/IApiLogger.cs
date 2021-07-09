using System;
using System.Dynamic;

namespace GuimoSoft.Logger
{
    public interface IApiLogger
    {
        ILoggerBuilder ComPropriedade<T>(string key, T value);
        ExpandoObject CriarDicionarioDeLog();
        void Rastreio(string mensagem);
        void Rastreio(ExpandoObject log);
        void Depuracao(string mensagem);
        void Depuracao(ExpandoObject log);
        void Informacao(string mensagem);
        void Informacao(ExpandoObject log);
        void Atencao(string mensagem);
        void Atencao(ExpandoObject log);
        void Erro(string mensagem);
        void Erro(ExpandoObject log);
        void Erro(Exception excecao);
        void Erro(string mensagem, Exception excecao);
    }
    public interface IApiLogger<out TCategoryName> : IApiLogger
    {
    }
}

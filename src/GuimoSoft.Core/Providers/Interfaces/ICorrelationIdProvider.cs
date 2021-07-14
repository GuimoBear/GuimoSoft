namespace GuimoSoft.Core.Providers.Interfaces
{
    public interface ICorrelationIdProvider
    {
        CorrelationId Get();
        void SetCorrelationIdInResponseHeader();
    }
}

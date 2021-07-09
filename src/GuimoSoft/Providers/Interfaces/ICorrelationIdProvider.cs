namespace GuimoSoft.Providers.Interfaces
{
    public interface ICorrelationIdProvider
    {
        CorrelationId Get();
        void SetCorrelationIdInResponseHeader();
    }
}

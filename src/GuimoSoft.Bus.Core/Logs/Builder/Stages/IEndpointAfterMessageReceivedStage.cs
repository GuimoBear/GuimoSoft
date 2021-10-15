namespace GuimoSoft.Bus.Core.Logs.Builder.Stages
{
    public interface IEndpointAfterEventReceivedStage
    {
        IWriteStage FromEndpoint(string endpoint);
    }
}

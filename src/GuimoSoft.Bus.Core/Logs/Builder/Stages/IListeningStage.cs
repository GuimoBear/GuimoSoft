namespace GuimoSoft.Bus.Core.Logs.Builder.Stages
{
    public interface IListeningStage
    {
        IEndpointStage WhileListening();
        IEventObjectInstance AfterReceived();
        IMessageStage Write();
    }
}
namespace GuimoSoft.Bus.Abstractions
{
    public interface IConsumeContextAccessor<TMessage> where TMessage : IMessage
    {
        ConsumeContext<TMessage> Context { get; set; }
    }
}

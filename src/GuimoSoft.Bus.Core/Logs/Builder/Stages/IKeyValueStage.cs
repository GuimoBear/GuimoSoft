namespace GuimoSoft.Bus.Core.Logs.Builder.Stages
{
    public interface IKeyValueStage
    {
        ILogLevelAndDataStage WithValue(object value);
    }
}

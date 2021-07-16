using System.Threading.Tasks;

namespace GuimoSoft.Bus.Core.Logs.Interfaces
{
    public interface IBusLogger
    {
        Task LogAsync(LogMessage log);

        Task ExceptionAsync(ExceptionMessage exception);
    }
}

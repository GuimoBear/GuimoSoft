using GuimoSoft.Bus.Core.Logs;
using GuimoSoft.Bus.Core.Logs.Interfaces;
using System.Threading.Tasks;

namespace GuimoSoft.Bus.Tests.Fakes
{
    public class FakeBusLogger : IBusLogger
    {
        public Task ExceptionAsync(ExceptionMessage exception)
            => Task.CompletedTask;

        public Task LogAsync(LogMessage log)
            => Task.CompletedTask;
    }
}

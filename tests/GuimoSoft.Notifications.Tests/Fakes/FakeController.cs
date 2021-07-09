using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace GuimoSoft.Notifications.Tests.Fakes
{
    public class FakeController : ControllerBase
    {
        public Task Execute([FromBody] FakeEntity fakeEntity)
            => Task.CompletedTask;
    }
}

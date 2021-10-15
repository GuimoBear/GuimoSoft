using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using GuimoSoft.Bus.Abstractions;
using GuimoSoft.Examples.Bus.Kafka.Events;

namespace GuimoSoft.Examples.Bus.Kafka.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HelloController : ControllerBase
    {
        private readonly IEventBus _bus;

        public HelloController(IEventBus bus)
        {
            _bus = bus;
        }

        [HttpGet]
        [Route("/{name}/{throwException}")]
        public async Task SayHello([FromRoute] string name, bool throwException)
        {
            await _bus.Publish(Guid.NewGuid().ToString(), new HelloEvent(name, throwException));
        }
    }
}

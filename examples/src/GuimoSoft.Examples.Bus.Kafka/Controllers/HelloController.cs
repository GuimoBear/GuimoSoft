using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using GuimoSoft.Bus.Abstractions;
using GuimoSoft.Examples.Bus.Kafka.Messages;

namespace GuimoSoft.Examples.Bus.Kafka.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HelloController : ControllerBase
    {
        private readonly IMessageProducer _producer;

        public HelloController(IMessageProducer producer)
        {
            _producer = producer;
        }

        [HttpGet]
        [Route("/{name}")]
        public async Task SayHello([FromRoute] string name)
        {
            await _producer.ProduceAsync(Guid.NewGuid().ToString(), new HelloMessage(name));
        }
    }
}

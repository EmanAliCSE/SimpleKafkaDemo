using KafkaWebApiDemo.Services;
using Microsoft.AspNetCore.Mvc;

namespace KafkaWebApiDemo.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class KafkaController : ControllerBase
    {
        private readonly KafkaProducerService _producerService;

        public KafkaController(KafkaProducerService producerService)
        {
            _producerService = producerService;
        }

        [HttpPost("send")]
        public async Task<IActionResult> Send([FromBody] string message)
        {
            var result = await _producerService.SendMessageAsync(message);
            return Ok(result);
        }
    }
}

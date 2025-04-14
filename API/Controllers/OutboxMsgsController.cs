using Domain.Models;
using Infrastructure.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OutboxMsgsController : ControllerBase
    {
        private readonly IUnitOfWork _uow;

        public OutboxMsgsController(IUnitOfWork uow)
        {
            _uow = uow;
        }
        [HttpGet]
        public async Task<ActionResult> GetAllOutbox()
        {
            var msgs = await _uow.Repository<OutboxMessage>().ListAsync();
            return Ok(msgs);
        }
    }
}

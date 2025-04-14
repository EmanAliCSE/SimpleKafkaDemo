using API.Controllers.Base;
using API.Features.Queries;
using Application.Features.Queries;
using Domain.Models;
using Infrastructure.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OutboxMsgsController : BaseApiController
    {
       

        public OutboxMsgsController()
        {
          
        }
        [HttpGet]
        public async Task<ActionResult> GetAllOutbox()
        {
            return Ok(await Mediator.Send(new GetOutboxMsgs.Query()));
        }
    }
}

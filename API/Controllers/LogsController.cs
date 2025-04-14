using API.Controllers.Base;
using Application.Features.Queries;
using Domain.Entities;
using Infrastructure.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LogsController : BaseApiController
    {
       
        public LogsController()
        {
         
        }
        [HttpGet("{actionId}")]
        public async Task<IActionResult> GetLogsByActionId(string actionId)
        {
            try
            {
                return Ok(await Mediator.Send(new GetLogs.Query() { Id=actionId }));

            }
            catch (Exception ex)
            {

                throw;
            }
        }

    }
}

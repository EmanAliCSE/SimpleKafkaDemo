using Domain.Entities;
using Infrastructure.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LogsController : ControllerBase
    {
        private readonly IUnitOfWork _uow;

        public LogsController(IUnitOfWork unitOfWork)
        {
            _uow = unitOfWork;
        }
        [HttpGet("{actionId}")]
        public async Task<IActionResult> GetLogsByActionId(string actionId)
        {
            try
            {

           
          // var temp= await _uow.Repository<Log>().FirstOrDefaultAsync(a=>a.Id==15);
            var logs = await _uow.Repository<Log>()
                .FindByCondition(log => log.ActionId.ToString()==actionId)
                .OrderByDescending(log => log.TimeStamp)
                .ToListAsync();

            return Ok(logs);
            }
            catch (Exception ex)
            {

                throw;
            }
        }

    }
}

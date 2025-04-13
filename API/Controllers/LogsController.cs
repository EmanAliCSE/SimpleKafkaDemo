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
        [HttpGet("booking/{bookingId}")]
        public async Task<IActionResult> GetLogsByBookingId(string bookingId)
        {
            var logs = await _uow.Repository<Log>()
                .FindByCondition(log => log.Properties.Contains(bookingId))
                .OrderByDescending(log => log.TimeStamp)
                .ToListAsync();

            return Ok(logs);
        }

        [HttpGet("correlation/{correlationId}")]
        public async Task<IActionResult> GetLogsByCorrelationId(string correlationId)
        {
            var logs = await _uow.Repository<Log>()
                .FindByCondition(log => log.Properties.Contains(correlationId))
                .OrderByDescending(log => log.TimeStamp)
                .ToListAsync();

            return Ok(logs);
        }
    }
}

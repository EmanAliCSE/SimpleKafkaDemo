using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;
using Infrastructure.Interfaces;

namespace Infrastructure.Services
{
    public class DeadLetterService : IDeadLetterService
    {
        private readonly IUnitOfWork _uow;

        public DeadLetterService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task SaveAsync(string topic, string key, string messageType, string message, string? reason="", string? payload = "", string? exceptionMessage = "")
        {
            var deadLetter = new DeadLetterMessage
            {
                Message = message,
                ExceptionMessage = exceptionMessage,
                Reason = reason,

                Topic = topic,

                MessageType = messageType,
                Key = key,
                Payload = payload,


            };

             _uow.Repository<DeadLetterMessage>().Add(deadLetter);
            await _uow.SaveChangesAsync();
        }
    }

}

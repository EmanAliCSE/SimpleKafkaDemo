using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using API.Models;
using Domain.Entities;
using Infrastructure.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Queries
{
    public class GetLogs
    {
        public class Query : IRequest<IEnumerable<Log>>
        {
            public string Id { get; set; }
        }

        public class Handler : IRequestHandler<Query, IEnumerable<Log>>
        {
            private readonly IUnitOfWork _uof;

            public Handler(IUnitOfWork uof)
            {
                _uof = uof;
            }

            public async Task<IEnumerable<Log>> Handle(Query request, CancellationToken cancellationToken)
            {
                var logs = await _uof.Repository<Log>()
                 .FindByCondition(log => log.ActionId.ToString() == request.Id)
                 .OrderByDescending(log => log.TimeStamp)
                 .ToListAsync();
                return logs;
            }
        }
    }
}

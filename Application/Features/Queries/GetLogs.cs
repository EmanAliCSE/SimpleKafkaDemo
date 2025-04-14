using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using API.Models;
using Application.DTO;
using AutoMapper;
using Domain.Entities;
using Infrastructure.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Queries
{
    public class GetLogs
    {
        public class Query : IRequest<IEnumerable<LogDTO>>
        {
            public string Id { get; set; }
        }

        public class Handler : IRequestHandler<Query, IEnumerable<LogDTO>>
        {
            private readonly IUnitOfWork _uof;
            private readonly IMapper _mapper;

            public Handler(IUnitOfWork uof, IMapper mapper)
            {
                _mapper = mapper;
                _uof = uof;
            }

            public async Task<IEnumerable<LogDTO>> Handle(Query request, CancellationToken cancellationToken)
            {
                var logs = await _uof.Repository<Log>()
                 .FindByCondition(log => log.ActionId.ToString() == request.Id)
                 .OrderByDescending(log => log.TimeStamp)
                 .ToListAsync();
                var dto = _mapper.Map<IEnumerable<LogDTO>>(logs);
                return dto;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using API.Models;
using Application.DTO;
using AutoMapper;
using Domain.Entities;
using Domain.Models;
using Infrastructure.Interfaces;
using MediatR;

namespace Application.Features.Queries
{
    public class GetOutboxMsgs
    {
        public class Query : IRequest<IEnumerable<OutboxMessageDTO>>
        {
        }

        public class Handler : IRequestHandler<Query, IEnumerable<OutboxMessageDTO>>
        {
            private readonly IUnitOfWork _uof;
            private readonly IMapper _mapper;

            public Handler(IUnitOfWork uof, IMapper mapper)
            {
                _mapper = mapper;
                _uof = uof;
            }

            public async Task<IEnumerable<OutboxMessageDTO>> Handle(Query request, CancellationToken cancellationToken)
            {
              var msgs = await _uof.Repository<OutboxMessage>().ListAsync();
                var dto = _mapper.Map<IEnumerable<OutboxMessageDTO>>(msgs);
                return dto;
            }
        }
    }
}

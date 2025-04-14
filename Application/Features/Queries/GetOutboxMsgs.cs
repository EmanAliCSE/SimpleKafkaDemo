using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using API.Models;
using Domain.Models;
using Infrastructure.Interfaces;
using MediatR;

namespace Application.Features.Queries
{
    public class GetOutboxMsgs
    {
        public class Query : IRequest<IEnumerable<OutboxMessage>>
        {
        }

        public class Handler : IRequestHandler<Query, IEnumerable<OutboxMessage>>
        {
            private readonly IUnitOfWork _uof;

            public Handler(IUnitOfWork uof)
            {
                _uof = uof;
            }

            public async Task<IEnumerable<OutboxMessage>> Handle(Query request, CancellationToken cancellationToken)
            {
                return await _uof.Repository<OutboxMessage>().ListAsync();
            }
        }
    }
}

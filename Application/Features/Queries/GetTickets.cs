using API.Models;
using Infrastructure.Interfaces;
using MediatR;

namespace API.Features.Queries
{
    public class GetTickets
    {
        public class Query : IRequest<IEnumerable<TicketBooking>>
        {
        }

        public class Handler : IRequestHandler<Query, IEnumerable<TicketBooking>>
        {
            private readonly IUnitOfWork _uof;

            public Handler(IUnitOfWork uof)
            {
                _uof=uof;
            }

            public async Task<IEnumerable<TicketBooking>> Handle(Query request, CancellationToken cancellationToken)
            {
                return await _uof.Repository<TicketBooking>().ListAsync();
            }
        }
    }
}

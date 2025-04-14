using API.DTO;
using API.Models;
using Application.DTO;
using AutoMapper;
using Infrastructure.Interfaces;
using MediatR;

namespace API.Features.Queries
{
    public class GetTickets
    {
        public class Query : IRequest<IEnumerable<TicketDTO>>
        {
        }

        public class Handler : IRequestHandler<Query, IEnumerable<TicketDTO>>
        {
            private readonly IUnitOfWork _uof;
            private readonly IMapper _mapper;

            public Handler(IUnitOfWork uof, IMapper mapper)
            {
                _mapper = mapper;
                _uof = uof;
            }

            public async Task<IEnumerable<TicketDTO>> Handle(Query request, CancellationToken cancellationToken)
            {
               var tickets= await _uof.Repository<TicketBooking>().ListAsync();
               
                var dto = _mapper.Map<IEnumerable<TicketDTO>>(tickets);
                return dto;
            }
        }
    }
}

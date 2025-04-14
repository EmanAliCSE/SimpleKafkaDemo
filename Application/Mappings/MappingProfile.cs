using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using API.DTO;
using API.Models;
using Application.DTO;
using AutoMapper;
using Domain.Entities;
using Domain.Models;

namespace Application.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Log, LogDTO>();
            CreateMap<OutboxMessage, OutboxMessageDTO>();
            CreateMap<TicketBooking, TicketDTO>();
        }
    }
}

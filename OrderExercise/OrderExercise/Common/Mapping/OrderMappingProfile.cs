using AutoMapper;
using Features.Orders;
using Features.Orders.DTOs;
using Features.Orders.Requests;

namespace Common.Mapping
{
    public class OrderMappingProfile : Profile
    {
        public OrderMappingProfile()
        {
            CreateMap<CreateOrderProfileRequest, Order>();
            CreateMap<Order, OrderProfileDto>();
        }
    }
}
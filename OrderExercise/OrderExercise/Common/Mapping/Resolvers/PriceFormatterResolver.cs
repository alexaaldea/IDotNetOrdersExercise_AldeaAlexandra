using AutoMapper;
using Features.Orders;
using Features.Orders.DTOs;

public class PriceFormatterResolver : IValueResolver<Order, OrderProfileDto, string>
{
    public string Resolve(Order source, OrderProfileDto destination, string destMember, ResolutionContext context)
        => source.Price.ToString("C2");
}
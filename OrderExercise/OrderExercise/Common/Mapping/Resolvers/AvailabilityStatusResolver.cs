using AutoMapper;

using Features.Orders;
using Features.Orders.DTOs;

public class AvailabilityStatusResolver : IValueResolver<Order, OrderProfileDto, string>
{
    public string Resolve(Order source, OrderProfileDto destination, string destMember, ResolutionContext context)
    {
        if (!source.IsAvailable)
            return "Out of Stock";


        if (source.StockQuantity == 0)
            return "Unavailable";
        if (source.StockQuantity == 1)
            return "Last Copy";
        if (source.StockQuantity <= 5)
            return "Limited Stock";


        return "In Stock";
    }
}
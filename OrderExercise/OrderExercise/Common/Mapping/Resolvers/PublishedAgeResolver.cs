using AutoMapper;
using Features.Orders;
using Features.Orders.DTOs;

public class PublishedAgeResolver : IValueResolver<Order, OrderProfileDto, string>
{
    public string Resolve(Order source, OrderProfileDto destination, string destMember, ResolutionContext context)
    {
        var days = (DateTime.UtcNow - source.PublishedDate).TotalDays;


        if (days < 30)
            return "New Release";
        if (days < 365)
            return $"{(int)(days / 30)} months old";
        if (days < 1825)
            return $"{(int)(days / 365)} years old";
        if (days == 1825)
            return "Classic";


        return $"{(int)(days / 365)} years old";
    }
}
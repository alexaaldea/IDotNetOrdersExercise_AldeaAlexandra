using AutoMapper;
using OrderExercise.Models.Entities;
using OrderExercise.Models.Dtos;

namespace OrderExercise.Models.Mapping.Resolvers
{
    public class PriceFormatterResolver : IValueResolver<Order, OrderProfileDto, string>
    {
        public String Resolve(Order source, OrderProfileDto destination, string destMember, ResolutionContext context)
        {
            return source.Price.ToString("C2");
        }
    }
}
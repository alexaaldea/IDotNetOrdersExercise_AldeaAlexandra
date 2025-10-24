using AutoMapper;
using OrderExercise.Models.Entities;
using OrderExercise.Models.Dtos;

namespace OrderExercise.Models.Mapping.Resolvers
{
    public class CategoryDisplayResolver : IValueResolver<Order, OrderProfileDto, string>
    {
        public string Resolve(Order source, OrderProfileDto destination, string destMember, ResolutionContext context)
        {
            return source.Category switch
            {
                Models.Enums.OrderCategory.Fiction => "Fiction & Literature",
                Models.Enums.OrderCategory.NonFiction => "Non-Fiction",
                Models.Enums.OrderCategory.Technical => "Technical & Professional",
                Models.Enums.OrderCategory.Children => "Children's Orders",
                _ => "Uncategorized"
            };
        }
    }
}
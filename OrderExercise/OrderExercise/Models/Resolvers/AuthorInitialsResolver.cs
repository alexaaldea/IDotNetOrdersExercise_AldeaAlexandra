using AutoMapper;
using OrderExercise.Models.Entities;
using OrderExercise.Models.Dtos;
using System.Linq;

namespace OrderExercise.Models.Mapping.Resolvers
{
    public class AuthorInitialsResolver : IValueResolver<Order, OrderProfileDto, string>
    {
        public string Resolve(Order source, OrderProfileDto destination, string destMember, ResolutionContext context)
        {
            if (string.IsNullOrWhiteSpace(source.Author))
                return "?";

            var names = source.Author.Split(' ', System.StringSplitOptions.RemoveEmptyEntries);
            if (names.Length >= 2)
                return (names.First()[0].ToString() + names.Last()[0].ToString()).ToUpper();
            
            return names[0][0].ToString().ToUpper();
        }
    }
}
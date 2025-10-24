using AutoMapper;
using OrderExercise.Models.Entities;
using OrderExercise.Models.Dtos;
using System;

namespace OrderExercise.Models.Mapping.Resolvers
{
    public class PublishedAgeResolver : IValueResolver<Order, OrderProfileDto, string>
    {
        public string Resolve(Order source, OrderProfileDto destination, string destMember, ResolutionContext context)
        {
            var daysOld = (DateTime.UtcNow - source.PublishedDate).TotalDays;
            
            if(daysOld <30)
                return "New Release";
            if (daysOld < 365)
                return $"{Math.Floor(daysOld / 30)} months old";
            if (daysOld < 1825)
                return $"{Math.Floor(daysOld / 365)} years old";
            return "Classic";
        }
    }
}
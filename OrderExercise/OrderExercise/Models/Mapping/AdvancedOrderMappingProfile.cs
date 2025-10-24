using System;
using AutoMapper;
using OrderExercise.Models.Entities;
using OrderExercise.Models.Requests;
using OrderExercise.Models.Dtos;
using OrderExercise.Models.Mapping.Resolvers; 

public class AdvancedOrderMappingProfile : Profile
{
    public AdvancedOrderMappingProfile()
    {
        CreateMap<CreateOrderProfileRequest, Order>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(_ => Guid.NewGuid()))           
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))  
            .ForMember(dest => dest.IsAvailable, opt => opt.MapFrom(src => src.StockQuantity > 0)) 
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());                       
        CreateMap<Order, OrderProfileDto>()
            .ForMember(dest => dest.CategoryDisplayName, opt => opt.MapFrom<CategoryDisplayResolver>())
            .ForMember(dest => dest.FormattedPrice, opt => opt.MapFrom<PriceFormatterResolver>())
            .ForMember(dest => dest.PublishedAge, opt => opt.MapFrom<PublishedAgeResolver>())
            .ForMember(dest => dest.AuthorInitials, opt => opt.MapFrom<AuthorInitialsResolver>())
            .ForMember(dest => dest.AvailabilityStatus, opt => opt.MapFrom<AvailabilityStatusResolver>())
            .ForMember(dest => dest.CoverImageUrl, opt => opt.MapFrom(src =>
                src.Category == OrderExercise.Models.Enums.OrderCategory.Children 
                    ? null 
                    : src.CoverImageUrl
            ))
            .ForMember(dest => dest.Price, opt => opt.MapFrom(src =>
                src.Category == OrderExercise.Models.Enums.OrderCategory.Children
                    ? src.Price * 0.9m
                    : src.Price
            ));
    }
}
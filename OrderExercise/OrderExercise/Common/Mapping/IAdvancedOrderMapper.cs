using Features.Orders;
using Features.Orders.DTOs;

namespace Common.Mapping
{
    public interface IAdvancedOrderMapper
    {
        Order MapToOrder(CreateOrderProfileRequest request);
        OrderProfileDto MapToOrderProfileDto(Order order);
    }
}
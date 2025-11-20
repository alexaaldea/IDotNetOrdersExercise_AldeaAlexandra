using Features.Orders;
using Features.Orders.DTOs;
using Features.Orders.Requests;

namespace Common.Mapping
{
    public interface IAdvancedOrderMapper
    {
        Order MapToOrder(CreateOrderProfileRequest request);
        OrderProfileDto MapToOrderProfileDto(Order order);
    }
}
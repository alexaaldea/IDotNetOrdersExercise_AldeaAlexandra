using System;
using Features.Orders;

namespace Features.Orders.Logging
{
    public record OrderCreationMetrics(
        string OperationId,
        string OrderTitle,
        string ISBN,
        OrderCategory Category,
        TimeSpan ValidationDuration,
        TimeSpan DatabaseSaveDuration,
        TimeSpan TotalDuration,
        bool Success,
        string? ErrorReason = null
    );
}
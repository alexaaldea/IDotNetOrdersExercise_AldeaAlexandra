using Features.Orders;
using System;

namespace Common.Logging
{
    public record OrderCreationMetrics
    (
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
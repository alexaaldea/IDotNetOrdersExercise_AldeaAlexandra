using System;
using Microsoft.Extensions.Logging;
using Features.Orders; 

namespace Common.Logging
{
    public static class LoggingExtensions
    {
        public static void LogOrderCreationMetrics(
            this ILogger logger,
            OrderCreationMetrics metrics)
        {
            var validationMs = metrics.ValidationDuration.TotalMilliseconds;
            var dbMs = metrics.DatabaseSaveDuration.TotalMilliseconds;
            var totalMs = metrics.TotalDuration.TotalMilliseconds;
            var status = metrics.Success ? "SUCCESS" : "FAILURE";
            var error = metrics.ErrorReason ?? string.Empty;

            logger.LogInformation(
                eventId: new EventId(LogEvents.OrderCreationCompleted, "OrderCreationMetrics"),
                message: "Order '{Title}' [ISBN: {ISBN}, Category: {Category}] completed. " +
                         "Validation: {Validation}ms, DatabaseSave: {Database}ms, Total: {Total}ms, Status: {Status} {Error}",
                metrics.OrderTitle,
                metrics.ISBN,
                metrics.Category,
                validationMs,
                dbMs,
                totalMs,
                status,
                error
            );
        }
    }
}
using Microsoft.AspNetCore.Http;      
using Microsoft.Extensions.Logging;    
using System;
using System.Threading.Tasks;

public class CorrelationMiddleware
{
    private readonly RequestDelegate _next;

    public CorrelationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ILogger<CorrelationMiddleware> logger)
    {
        const string correlationHeader = "X-Correlation-ID";

        if (!context.Request.Headers.TryGetValue(correlationHeader, out var correlationId))
        {
            correlationId = Guid.NewGuid().ToString();
            context.Request.Headers[correlationHeader] = correlationId;
        }

        context.Items[correlationHeader] = correlationId;

        using (logger.BeginScope("{CorrelationId}", correlationId))
        {
            context.Response.Headers[correlationHeader] = correlationId;
            await _next(context);
        }
    }
}
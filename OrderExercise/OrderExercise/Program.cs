using Common.Logging;
using Common.Mapping;
using Features.Validators;
using Features.Orders;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Features.Orders.Requests;
using Microsoft.OpenApi;
using Microsoft.AspNetCore.Http;



var builder = WebApplication.CreateBuilder(args);


builder.Services.AddAutoMapper(typeof(OrderMappingProfile));
builder.Services.AddAutoMapper(typeof(AdvancedOrderMappingProfile));

builder.Services.AddScoped<IValidator<CreateOrderProfileRequest>, CreateOrderProfileValidator>();


builder.Services.AddValidatorsFromAssemblyContaining<CreateOrderProfileValidator>();


builder.Services.AddFluentValidationAutoValidation();


builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<CreateOrderHandler>();

builder.Services.AddMemoryCache();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var app = builder.Build();

app.UseMiddleware<CorrelationMiddleware>();

app.UseSwagger();
app.UseSwaggerUI();

app.MapPost("/orders", async (
    CreateOrderHandler handler,
    CreateOrderProfileRequest request) =>
{
    var result = await handler.HandleAsync(request);
    return Results.Ok(result);

})
    
.WithTags("Orders")  
.WithSummary("Creates a new order")  
.WithDescription("Creates an order with validation, metrics tracking, and correlation ID support.");

app.Run();

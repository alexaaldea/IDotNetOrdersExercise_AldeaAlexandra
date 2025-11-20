using AutoMapper;
using Features.Orders.DTOs;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Common.Logging;
using Microsoft.AspNetCore.Http;

using OrderRequest = Features.Orders.Requests.CreateOrderProfileRequest;

namespace Features.Orders
{
    public class CreateOrderHandler
    {
        private readonly IMapper _mapper;
        private readonly IOrderRepository _orderRepository;
        private readonly IMemoryCache _cache;
        private readonly ILogger<CreateOrderHandler> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CreateOrderHandler(
            IMapper mapper,
            IOrderRepository orderRepository,
            IMemoryCache cache,
            ILogger<CreateOrderHandler> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _mapper = mapper;
            _orderRepository = orderRepository;
            _cache = cache;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<OrderProfileDto> HandleAsync(OrderRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var operationId = Guid.NewGuid().ToString("N").Substring(0, 8);
            var totalStopwatch = Stopwatch.StartNew();

            var correlationId = _httpContextAccessor.HttpContext?.Request.Headers["X-Correlation-ID"].ToString();

            using var scope = _logger.BeginScope(new { OperationId = operationId, CorrelationId = correlationId });
            _logger.LogInformation(LogEvents.OrderCreationStarted,
                "Starting order creation: Title='{Title}', Author='{Author}', Category='{Category}', ISBN='{ISBN}'",
                request.Title, request.Author, request.Category, request.ISBN);

            var validationStopwatch = Stopwatch.StartNew();

            try
            {
                var existingOrder = await _orderRepository.GetByISBNAsync(request.ISBN);
                _logger.LogInformation(LogEvents.ISBNValidationPerformed,
                    "Performed ISBN validation for ISBN='{ISBN}'", request.ISBN);

                if (existingOrder != null)
                {
                    _logger.LogWarning(LogEvents.OrderValidationFailed,
                        "Order validation failed: ISBN '{ISBN}' already exists", request.ISBN);

                    var metricsFail = new OrderCreationMetrics(
                        operationId,
                        request.Title,
                        request.ISBN,
                        request.Category,
                        validationStopwatch.Elapsed,
                        TimeSpan.Zero,
                        totalStopwatch.Elapsed,
                        false, 
                        $"Order with ISBN '{request.ISBN}' already exists."
                    );


                    _logger.LogOrderCreationMetrics(metricsFail);
                    throw new InvalidOperationException($"Order with ISBN '{request.ISBN}' already exists.");
                }

                _logger.LogInformation(LogEvents.StockValidationPerformed,
                    "Performed stock validation for ISBN='{ISBN}'", request.ISBN);

                validationStopwatch.Stop();

                var order = _mapper.Map<Order>(request);

                var dbStopwatch = Stopwatch.StartNew();
                _logger.LogInformation(LogEvents.DatabaseOperationStarted,
                    "Starting database save for order Title='{Title}', ISBN='{ISBN}'",
                    order.Title, order.ISBN);

                await _orderRepository.AddAsync(order);

                dbStopwatch.Stop();
                _logger.LogInformation(LogEvents.DatabaseOperationCompleted,
                    "Database save completed for OrderId={OrderId}", order.Id);

                _cache.Remove("all_orders");
                _logger.LogInformation(LogEvents.CacheOperationPerformed,
                    "Cache 'all_orders' cleared after adding OrderId={OrderId}", order.Id);

                totalStopwatch.Stop();

                var metrics = new OrderCreationMetrics(
                    operationId,
                    order.Title,
                    order.ISBN,
                    order.Category,
                    validationStopwatch.Elapsed,
                    dbStopwatch.Elapsed,
                    totalStopwatch.Elapsed,
                    true 
                );

                _logger.LogOrderCreationMetrics(metrics);

                var orderDto = _mapper.Map<OrderProfileDto>(order);
                _logger.LogInformation(LogEvents.OrderCreationCompleted,
                    "Order creation completed successfully: OrderId={OrderId}", order.Id);

                return orderDto;
            }
            catch (Exception ex)
            {
                totalStopwatch.Stop();

                var metricsError = new OrderCreationMetrics(
                    operationId,
                    request.Title,
                    request.ISBN,
                    request.Category,
                    validationStopwatch.Elapsed,
                    TimeSpan.Zero,
                    totalStopwatch.Elapsed,
                    false, 
                    ex.Message
                );

                _logger.LogOrderCreationMetrics(metricsError);

                throw; 
            }
        }
    }
}

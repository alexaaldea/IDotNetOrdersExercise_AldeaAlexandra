using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using AutoMapper;
using Features.Orders;
using Features.Orders.Requests;
using Features.Orders.DTOs;
using Common.Mapping;
using Common.Logging;
using Xunit;
using Microsoft.AspNetCore.Http;

namespace Features.Tests.Orders
{
    public class CreateOrderHandlerIntegrationTests : IDisposable
    {
        private readonly IMemoryCache _cache;
        private readonly IMapper _mapper;
        private readonly Mock<ILogger<CreateOrderHandler>> _loggerMock;
        private readonly CreateOrderHandler _handler;
        private readonly IOrderRepository _repository;

        public CreateOrderHandlerIntegrationTests()
        {
            // Memory cache
            _cache = new MemoryCache(new MemoryCacheOptions());

            // AutoMapper configuration
            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<OrderMappingProfile>();
                cfg.AddProfile<AdvancedOrderMappingProfile>();
            });
            _mapper = mapperConfig.CreateMapper();

            // Logger mock
            _loggerMock = new Mock<ILogger<CreateOrderHandler>>();

            // In-memory repository
            _repository = new OrderRepository();

            // Mock IHttpContextAccessor
            var httpContextMock = new Mock<IHttpContextAccessor>();
            var context = new DefaultHttpContext();
            context.Request.Headers["X-Correlation-ID"] = "test-correlation-id";
            httpContextMock.Setup(a => a.HttpContext).Returns(context);

            // Handler instance
            _handler = new CreateOrderHandler(
                _mapper,
                _repository,
                _cache,
                _loggerMock.Object,
                httpContextMock.Object);
        }


        [Fact]
        public async Task Handle_ValidTechnicalOrderRequest_CreatesOrderWithCorrectMappings()
        {
            // Arrange
            var request = new CreateOrderProfileRequest
            {
                Title = "Advanced C# Techniques",
                Author = "John Smith",
                ISBN = "1234567890",
                Category = OrderCategory.Technical,
                Price = 50,
                PublishedDate = DateTime.Today.AddYears(-1),
                StockQuantity = 5,
                CoverImageUrl = "https://example.com/book.jpg"
            };

            // Act
            var result = await _handler.HandleAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Technical & Professional", result.CategoryDisplayName);
            Assert.Equal("JS", result.AuthorInitials);
            Assert.Equal("1 years old", result.PublishedAge);
            Assert.StartsWith("50", result.FormattedPrice);
            Assert.EndsWith("lei", result.FormattedPrice);
            Assert.Equal("Limited Stock", result.AvailabilityStatus);
            
            LoggerExtensions.VerifyLog(_loggerMock, LogLevel.Information, LogEvents.OrderCreationStarted, Times.AtLeastOnce());
            
        }

        [Fact]
        public async Task Handle_DuplicateISBN_ThrowsValidationExceptionWithLogging()
        {
            // Arrange: Add existing order
            var existingOrder = new Order
            {
                Title = "Existing Book",
                Author = "Jane Doe",
                ISBN = "1111111111",
                Category = OrderCategory.Technical,
                Price = 30,
                PublishedDate = DateTime.Today.AddYears(-1),
                StockQuantity = 5
            };
            await _repository.AddAsync(existingOrder);

            var request = new CreateOrderProfileRequest
            {
                Title = "New Book",
                Author = "John Smith",
                ISBN = "1111111111",
                Category = OrderCategory.Technical,
                Price = 40,
                PublishedDate = DateTime.Today,
                StockQuantity = 3
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.HandleAsync(request));
            Assert.Contains("already exists", ex.Message);
            
            LoggerExtensions.VerifyLog(_loggerMock, LogLevel.Warning, LogEvents.OrderValidationFailed, Times.Once());

        }

        [Fact]
        public async Task Handle_ChildrensOrderRequest_AppliesDiscountAndConditionalMapping()
        {
            // Arrange
            var request = new CreateOrderProfileRequest
            {
                Title = "Fun Stories",
                Author = "Alice Wonderland",
                ISBN = "2222222222",
                Category = OrderCategory.Children,
                Price = 20,
                PublishedDate = DateTime.Today.AddYears(-2),
                StockQuantity = 15,
                CoverImageUrl = "https://example.com/kids.png"
            };

            // Act
            var result = await _handler.HandleAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Children's Orders", result.CategoryDisplayName);
            Assert.Equal(18, result.Price); // Assuming 10% discount applied in mapping
            Assert.Null(result.CoverImageUrl);
            
            LoggerExtensions.VerifyLog(_loggerMock, LogLevel.Information, LogEvents.OrderCreationCompleted, Times.AtLeastOnce());

        }

        public void Dispose()
        {
            _cache?.Dispose();
        }
    }

    public static class LoggerExtensions
    {
        public static void VerifyLog(
            this Mock<ILogger<CreateOrderHandler>> loggerMock,
            LogLevel level,
            int eventId,
            Times times)
        {
            loggerMock.Verify(
                x => x.Log(
                    level,
                    It.Is<EventId>(e => e.Id == eventId),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception?>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                times);
        }

    }
}

using OrderExercise.Models.Enums;

namespace OrderExercise.Models.Requests
{
    public class CreateOrderProfileRequest
    {
        public string Title { get; set; } = string.Empty;

        public string Author { get; set; } = string.Empty;

        public string ISBN { get; set; } = string.Empty;

        public OrderCategory Category { get; set; }

        public decimal Price { get; set; }

        public DateTime PublishedDate { get; set; }

        public string? CoverImageUrl { get; set; }

        public int StockQuantity { get; set; } = 1;
    }
}
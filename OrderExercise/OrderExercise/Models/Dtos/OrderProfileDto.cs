namespace OrderExercise.Models.Dtos
{
    public class OrderProfileDto
    {
        public Guid ID { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string ISBN { get; set; }
        public string CategoryDisplayName { get; set; }
        public decimal Price { get; set; }
        public string FormattedPrice { get; set; }
        public DateTime PublishedDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CoverImageUrl { get; set; }
        public bool isAvailable { get; set; }
        public int StockQuantity { get; set; } = 0;
        public string PublishedAge { get; set; }
        public string AuthorInitials { get; set; }
        public string AvailabilityStatus { get; set; }
    }
}
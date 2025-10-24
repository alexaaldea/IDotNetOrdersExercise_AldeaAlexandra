using OrderExercise.Models.Enums;
using System;

namespace OrderExercise.Models.Entities
{
    public class Order
    {
        public Guid Id { get; set; }                     
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string ISBN { get; set; } = string.Empty;
        public OrderCategory Category { get; set; }
        public decimal Price { get; set; }
        public DateTime PublishedDate { get; set; }
        public string? CoverImageUrl { get; set; }
        public bool IsAvailable { get; set; }            
        public int StockQuantity { get; set; } = 0;
        public DateTime CreatedAt { get; set; }          
        public DateTime? UpdatedAt { get; set; }         
    }
}
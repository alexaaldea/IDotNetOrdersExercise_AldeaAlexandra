using Features.Orders.Requests;
using FluentValidation;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Features.Validators
{
    public class CreateOrderProfileValidator : AbstractValidator<CreateOrderProfileRequest>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ILogger<CreateOrderProfileValidator> _logger;

        // Inappropriate content lists
        private readonly List<string> _inappropriateWords = new() { "badword1", "badword2" };
        private readonly List<string> _childrenRestrictedWords = new() { "violent", "adult", "horror" };

        public CreateOrderProfileValidator(
            IOrderRepository orderRepository,
            ILogger<CreateOrderProfileValidator> logger)
        {
            _orderRepository = orderRepository;
            _logger = logger;

            // TITLE RULES
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title cannot be empty.")
                .Length(1, 200).WithMessage("Title must be between 1 and 200 characters.")
                .Must(BeValidTitle).WithMessage("Title contains inappropriate words.")
                .MustAsync(BeUniqueTitle).WithMessage("Title must be unique for the same author.");

            // AUTHOR RULES
            RuleFor(x => x.Author)
                .NotEmpty().WithMessage("Author cannot be empty.")
                .Length(2, 100).WithMessage("Author must be between 2 and 100 characters.")
                .Must(BeValidAuthorName).WithMessage("Author contains invalid characters.");

            // ISBN RULES
            RuleFor(x => x.ISBN)
                .NotEmpty().WithMessage("ISBN cannot be empty.")
                .Must(BeValidISBN).WithMessage("ISBN must be valid 10 or 13 digits format.")
                .MustAsync(BeUniqueISBN).WithMessage("ISBN already exists.");

            RuleFor(x => x.Category)
                .IsInEnum().WithMessage("Category is invalid.");

            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("Price must be greater than zero.")
                .LessThan(10000).WithMessage("Price must be less than $10,000.");

            RuleFor(x => x.PublishedDate)
                .LessThanOrEqualTo(DateTime.Today)
                .WithMessage("Published date cannot be in the future.")
                .GreaterThanOrEqualTo(new DateTime(1400, 1, 1))
                .WithMessage("Published date cannot be before year 1400.");

            RuleFor(x => x.StockQuantity)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Stock cannot be negative.")
                .LessThanOrEqualTo(100_000)
                .WithMessage("Stock quantity exceeds allowed maximum.");

            RuleFor(x => x.CoverImageUrl)
                .Must(BeValidImageUrl)
                .When(x => !string.IsNullOrWhiteSpace(x.CoverImageUrl))
                .WithMessage("CoverImageUrl must be a valid image URL.");

            // BUSINESS RULES
            RuleFor(x => x)
                .MustAsync(PassBusinessRules)
                .WithMessage("Business rule validation failed.");

            // TECHNICAL ORDERS
            When(x => x.Category.ToString() == "Technical", () =>
            {
                RuleFor(x => x.Price)
                    .GreaterThanOrEqualTo(20)
                    .WithMessage("Technical orders must cost at least $20.00.");

                RuleFor(x => x.Title)
                    .Must(ContainTechnicalKeywords)
                    .WithMessage("Technical orders must include technical-related keywords in the title.");

                RuleFor(x => x.PublishedDate)
                    .Must(BeWithinLast5Years)
                    .WithMessage("Technical orders must be published within the last 5 years.");
            });

            // CHILDREN’S ORDERS
            When(x => x.Category.ToString() == "Children", () =>
            {
                RuleFor(x => x.Price)
                    .LessThanOrEqualTo(50)
                    .WithMessage("Children’s books must cost no more than $50.00.");

                RuleFor(x => x.Title)
                    .Must(BeAppropriateForChildren)
                    .WithMessage("Children’s book titles must not contain inappropriate words.");
            });

            // FICTION ORDERS
            When(x => x.Category.ToString() == "Fiction", () =>
            {
                RuleFor(x => x.Author)
                    .Must(author => author.Length >= 5)
                    .WithMessage("Fiction authors must have at least 5 characters in their full name.");
            });

            // CROSS-FIELD VALIDATION
            RuleFor(x => x)
                .Must(x => !(x.Price > 100 && x.StockQuantity > 20))
                .WithMessage("Orders costing more than $100 must not exceed 20 units in stock.");

            RuleFor(x => x)
                .Must(x => x.Category.ToString() != "Technical" || BeWithinLast5Years(x.PublishedDate))
                .WithMessage("Technical orders must be published within the last 5 years.");

        }
        
        // Inappropriate words check
        private bool BeValidTitle(string title)
        {
            bool valid = !_inappropriateWords
                .Any(word => title.Contains(word, StringComparison.OrdinalIgnoreCase));

            return valid;
        }

        // Unique Title per Author
        private async Task<bool> BeUniqueTitle(CreateOrderProfileRequest request, string title, CancellationToken ct)
        {
            _logger.LogInformation("Validating unique title '{Title}' for author '{Author}'...", title, request.Author);

            bool exists = await _orderRepository.ExistsTitleForAuthorAsync(title, request.Author);
            return !exists;
        }




        // Valid Author Name
        private bool BeValidAuthorName(string author)
        {
            return Regex.IsMatch(author, @"^[\p{L} \-'.]+$");
        }

        // Valid ISBN format
        private bool BeValidISBN(string isbn)
        {
            string digitsOnly = isbn.Replace("-", "");
            return (digitsOnly.Length == 10 || digitsOnly.Length == 13)
                   && long.TryParse(digitsOnly, out _);
        }

        // Unique ISBN
        private async Task<bool> BeUniqueISBN(string isbn, CancellationToken ct)
        {
            _logger.LogInformation("Validating unique ISBN '{ISBN}'...", isbn);

            bool exists = await _orderRepository.ExistsISBNAsync(isbn);

            return !exists;
        }

        // Valid Image URL
        private bool BeValidImageUrl(string url)
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out Uri? uri))
                return false;

            var validExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

            return (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)
                && validExtensions.Any(ext =>
                    uri.AbsolutePath.EndsWith(ext, StringComparison.OrdinalIgnoreCase));
        }

        // Full Business Rules
        private async Task<bool> PassBusinessRules(CreateOrderProfileRequest request, CancellationToken ct)
        {
            // Rule 1: Daily limit 500
            int ordersToday = await _orderRepository.CountOrdersAddedTodayAsync();
            if (ordersToday >= 500)
            {
                _logger.LogWarning("Business rule failed: Daily order limit exceeded.");
                return false;
            }

            // Rule 2: Technical orders must cost at least $20
            if (request.Category.ToString() == "Technical" && request.Price < 20)
            {
                _logger.LogWarning("Business rule failed: Technical order price too low.");
                return false;
            }

            // Rule 3: Children's content restricted
            if (request.Category.ToString() == "Children"
                && _childrenRestrictedWords.Any(w =>
                    request.Title.Contains(w, StringComparison.OrdinalIgnoreCase)))
            {
                _logger.LogWarning("Business rule failed: Restricted word found in Children title.");
                return false;
            }

            // Rule 4: Price > 500 => Max stock 10
            if (request.Price > 500 && request.StockQuantity > 10)
            {
                _logger.LogWarning("Business rule failed: High-value order has too much stock.");
                return false;
            }

            return true;
        }
        // ----------------------
// HELPER METHODS
// ----------------------

        private readonly List<string> _technicalKeywords = new()
        {
            "software", "programming", "cloud", "database", "system",
            "ai", "algorithm", "machine learning", "engineering", "devops"
        };

        private bool ContainTechnicalKeywords(string title)
        {
            return _technicalKeywords.Any(kw =>
                title.Contains(kw, StringComparison.OrdinalIgnoreCase));
        }

        private bool BeAppropriateForChildren(string title)
        {
            return !_childrenRestrictedWords.Any(w =>
                title.Contains(w, StringComparison.OrdinalIgnoreCase));
        }

        private bool BeWithinLast5Years(DateTime date)
        {
            return date >= DateTime.Today.AddYears(-5);
        }

    }
}

using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Validators.Attributes
{
    public class OrderCategoryAttribute : ValidationAttribute
    {
        private readonly string[] _allowed;

        public OrderCategoryAttribute(params string[] allowedCategories)
        {
            _allowed = allowedCategories;

            ErrorMessage = $"Category must be one of: {string.Join(", ", allowedCategories)}.";
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null)
                return ValidationResult.Success;

            string category = value.ToString()!;

            bool valid = _allowed.Contains(category, StringComparer.OrdinalIgnoreCase);

            return valid
                ? ValidationResult.Success
                : new ValidationResult(ErrorMessage);
        }
    }
}
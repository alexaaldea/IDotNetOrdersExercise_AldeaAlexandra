using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace Validators.Attributes
{
    public class PriceRangeAttribute : ValidationAttribute
    {
        private readonly decimal _min;
        private readonly decimal _max;

        public PriceRangeAttribute(double min, double max)
        {
            _min = Convert.ToDecimal(min);
            _max = Convert.ToDecimal(max);

            ErrorMessage =
                $"Price must be between {_min.ToString("C", CultureInfo.CurrentCulture)} " +
                $"and {_max.ToString("C", CultureInfo.CurrentCulture)}.";
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is null)
                return ValidationResult.Success;

            if (value is not decimal price)
                return new ValidationResult("Invalid price format.");

            if (price < _min || price > _max)
                return new ValidationResult(ErrorMessage);

            return ValidationResult.Success;
        }
    }
}
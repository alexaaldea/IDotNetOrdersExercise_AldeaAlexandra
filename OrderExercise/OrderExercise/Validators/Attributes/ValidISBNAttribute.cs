using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Validators.Attributes
{
    public class ValidISBNAttribute : ValidationAttribute, IClientModelValidator
    {
        public ValidISBNAttribute()
        {
            ErrorMessage = "ISBN must be a valid 10 or 13 digit number.";
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is null)
                return ValidationResult.Success;

            var isbn = value.ToString()!.Replace("-", "").Replace(" ", "");

            bool valid =
                (isbn.Length == 10 || isbn.Length == 13) &&
                isbn.All(char.IsDigit);

            return valid
                ? ValidationResult.Success
                : new ValidationResult(ErrorMessage);
        }

        public void AddValidation(ClientModelValidationContext context)
        {
            context.Attributes.Add("data-val", "true");
            context.Attributes.Add("data-val-isbn", ErrorMessage!);
        }
    }
}
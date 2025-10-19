using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace SavePlus_API.Attributes
{
    /// <summary>
    /// Validates phone number in E.164 format (+84xxxxxxxxx)
    /// </summary>
    public class ValidPhoneE164Attribute : ValidationAttribute
    {
        private static readonly Regex PhoneRegex = new Regex(
            @"^\+84[0-9]{9,10}$",
            RegexOptions.Compiled
        );

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            {
                // Null/empty is valid (use [Required] for mandatory fields)
                return ValidationResult.Success;
            }

            var phoneNumber = value.ToString()!.Trim();

            if (!PhoneRegex.IsMatch(phoneNumber))
            {
                return new ValidationResult(
                    ErrorMessage ?? "Số điện thoại phải có định dạng +84xxxxxxxxx (9-10 chữ số sau +84)",
                    new[] { validationContext.MemberName ?? "" }
                );
            }

            return ValidationResult.Success;
        }
    }
}


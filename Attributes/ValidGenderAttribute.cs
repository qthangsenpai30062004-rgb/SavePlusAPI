using System.ComponentModel.DataAnnotations;

namespace SavePlus_API.Attributes
{
    /// <summary>
    /// Validates that Gender is one of: M (Male), F (Female), O (Other)
    /// </summary>
    public class ValidGenderAttribute : ValidationAttribute
    {
        private static readonly string[] ValidGenders = { "M", "F", "O" };

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            {
                // Null/empty is valid (use [Required] for mandatory fields)
                return ValidationResult.Success;
            }

            var gender = value.ToString()!.Trim().ToUpper();

            if (!ValidGenders.Contains(gender))
            {
                return new ValidationResult(
                    ErrorMessage ?? "Giới tính phải là M (Nam), F (Nữ), hoặc O (Khác)",
                    new[] { validationContext.MemberName ?? "" }
                );
            }

            return ValidationResult.Success;
        }
    }
}

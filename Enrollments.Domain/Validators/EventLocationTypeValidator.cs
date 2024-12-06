using Enrollments.Domain.Enumerations;
using System.ComponentModel.DataAnnotations;

namespace Enrollments.Domain.Validators
{
    public class EventLocationTypeValidator : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value != null && !EventLocationTypeEnum.GetTypes().Contains(value.ToString()))
                return new ValidationResult("Invalid event location type.");

            return ValidationResult.Success;
        }
    }
}

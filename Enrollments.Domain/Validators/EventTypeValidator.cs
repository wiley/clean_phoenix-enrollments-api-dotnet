using Enrollments.Domain.Enumerations;
using System.ComponentModel.DataAnnotations;

namespace Enrollments.Domain.Validators
{
    public class EventTypeValidator : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value != null && !EventTypeEnum.GetTypes().Contains(value.ToString()))
                return new ValidationResult("Invalid event type.");

            return ValidationResult.Success;
        }
    }
}

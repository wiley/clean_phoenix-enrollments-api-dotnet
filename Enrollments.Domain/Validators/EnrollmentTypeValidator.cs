using System.ComponentModel.DataAnnotations;
using Enrollments.Domain.Enumerations;

namespace Enrollments.Domain.Validators
{
    public class EnrollmentTypeValidator : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if(value != null)
                if (!EnrollmentTypeEnum.GetTypes().Contains(value.ToString()))
                    return new ValidationResult("Invalid Enrollment Type.");

            return ValidationResult.Success;
        }
    }
}

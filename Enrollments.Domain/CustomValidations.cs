using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Enrollments.Domain
{
    public class CustomIdFormatValidation: ValidationAttribute {

        private static readonly string INVALID_ID_FORMAT = "The Id is in wrong format.";

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            string pattern = @"^[a-fA-F\d]{8}-(?:[a-fA-F\d]{4}-){3}[a-fA-F\d]{12}$";
            bool isValid = Regex.IsMatch(value.ToString(), pattern);

            if(!isValid)
               return new ValidationResult(INVALID_ID_FORMAT);

            return ValidationResult.Success;
        }

    }
}

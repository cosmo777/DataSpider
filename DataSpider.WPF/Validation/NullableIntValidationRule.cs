using System;
using System.Globalization;
using System.Windows.Controls;

namespace DataSpider.WPF.Validation
{
    public class NullableIntValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            var stringVal = value as string;
            if (String.IsNullOrWhiteSpace(stringVal))
            {
                return ValidationResult.ValidResult;
            }
            int intValue;
            if (int.TryParse(stringVal, out intValue))
            {
                return ValidationResult.ValidResult;
            }
            return new ValidationResult(false,"");
        }
    }

    public class IntValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            var stringVal = value as string;
            if (String.IsNullOrWhiteSpace(stringVal))
            {
                return new ValidationResult(false, "");
            }
            int intValue;
            if (int.TryParse(stringVal, out intValue))
            {
                return ValidationResult.ValidResult;
            }
            return new ValidationResult(false, "");
        }
    }
}

using System;
using System.Globalization;
using System.Windows.Controls;

namespace DataSpider.WPF.Validation
{
    public class NullableFloatValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            var stringVal = value as string;
            if (String.IsNullOrWhiteSpace(stringVal))
            {
                return ValidationResult.ValidResult;
            }
            float intValue;
            if (float.TryParse(stringVal, out intValue))
            {
                return ValidationResult.ValidResult;
            }
            return new ValidationResult(false, "");
        }
    }
}
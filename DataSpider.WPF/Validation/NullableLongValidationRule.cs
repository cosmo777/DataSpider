using System;
using System.Globalization;
using System.Windows.Controls;

namespace DataSpider.WPF.Validation
{
    public class NullableLongValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            var stringVal = value as string;
            if (String.IsNullOrWhiteSpace(stringVal))
            {
                return ValidationResult.ValidResult;
            }
            long intValue;
            if (long.TryParse(stringVal, out intValue))
            {
                return ValidationResult.ValidResult;
            }
            return new ValidationResult(false, "");
        }
    }
}
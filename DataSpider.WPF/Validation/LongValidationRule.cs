using System;
using System.Globalization;
using System.Windows.Controls;

namespace DataSpider.WPF.Validation
{
    public class LongValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            var stringVal = value as string;
            if (String.IsNullOrWhiteSpace(stringVal))
            {
                return new ValidationResult(false, "");
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
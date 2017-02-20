using System;

namespace Spartan.WPF.ValueConvertor
{
    public abstract class BaseValueConvertor
    {
        protected object Convert(object value, Type targetType)
        {
            if (targetType == typeof(long))
            {
                return ToLong(value);
            }
            if (targetType == typeof(long?))
            {
                return ToLong(value);
            }
            if (targetType == typeof(int))
            {
                return ToInt(value);
            }
            if (targetType == typeof(int?))
            {
                return ToInt(value);
            }
            if (targetType == typeof(string))
            {
                return ToString(value);
            }
            if (targetType == typeof(float?))
            {
                return ToFloat(value);
            }
            if (targetType == typeof(float))
            {
                return ToFloat(value);
            }
            return null;
        }

        protected long? ToLong(object value)
        {
            if (value == null)
            {
                return null;
            }
            if (value is long)
            {
                return (long)value;
            }
            if (!(value is string))
            {
                return null;
            }
            if (String.IsNullOrWhiteSpace((string)value))
            {
                return null;
            }
            long intValue;
            if (long.TryParse((string)value, out intValue))
            {
                return intValue;
            }
            return null;
        }

        protected int? ToInt(object value)
        {
            if (value == null)
            {
                return null;
            }
            if (value is int)
            {
                return (int)value;
            }
            if (!(value is string))
            {
                return null;
            }
            if (String.IsNullOrWhiteSpace((string)value))
            {
                return null;
            }
            int intValue;
            if (int.TryParse((string)value, out intValue))
            {
                return intValue;
            }
            return null;
        }
        protected float? ToFloat(object value)
        {
            if (value == null)
            {
                return null;
            }
            if (value is float)
            {
                return (float)value;
            }
            if (!(value is string))
            {
                return null;
            }
            if (String.IsNullOrWhiteSpace((string)value))
            {
                return null;
            }
            float intValue;
            if (float.TryParse(value as string, out intValue))
            {
                return intValue;
            }
            return null;
        }

        protected string ToString(object o)
        {
            if (o == null)
            {
                return null;
            }
            return o.ToString();
        }
    }
}
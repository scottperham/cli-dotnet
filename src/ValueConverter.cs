using System;
using System.Reflection;

namespace cli_dotnet
{
    public class ValueConverter : IValueConverter
    {
        object IValueConverter.GetValue(string value, Type type)
        {
            if (type == typeof(bool))
            {
                if (bool.TryParse(value, out var result))
                {
                    return result;
                }

                return true;
            }

            var isArray = type.IsArray;

            if (isArray)
            {
                type = type.GetElementType();
            }

            object actualValue;

            if (type.IsEnum)
            {
                actualValue = GetValueAsEnum(value, type);
            }
            else
            {
                actualValue = Convert.ChangeType(value, Type.GetTypeCode(type));
            }

            if (isArray)
            {
                var arr = Array.CreateInstance(type, 1);
                arr.SetValue(actualValue, 0);
                actualValue = arr;
            }

            return actualValue;
        }

        object GetValueAsEnum(string value, Type enumType)
        {
            if (int.TryParse(value, out var intValue))
            {
                return Enum.ToObject(enumType, intValue);
            }

            return Enum.Parse(enumType, value, true);
        }

        object IValueConverter.CreateDefaultValue(Type type)
        {
            if (type.IsArray)
            {
                return Array.CreateInstance(type.GetElementType(), 0);
            }

            if (type == typeof(string))
            {
                return "";
            }

            return Activator.CreateInstance(type);
        }
    }
}

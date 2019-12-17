using System;

namespace cli_dotnet
{
    public class ValueConverter : IValueConverter
    {
        object IValueConverter.GetValue(string value, Type type)
        {
            if (type == typeof(bool))
            {
                return true;
            }

            if (type.IsEnum)
            {
                if (!Enum.TryParse(type, value, true, out var result))
                {
                    throw new FormatException($"`{value}` is not a valid value");
                }

                return result;
            }

            var isArray = type.IsArray;

            if (isArray)
            {
                type = type.GetElementType();
            }

            var actualValue = Convert.ChangeType(value, Type.GetTypeCode(type));

            if (isArray)
            {
                var arr = Array.CreateInstance(type, 1);
                arr.SetValue(actualValue, 0);
                actualValue = arr;
            }

            return actualValue;
        }
    }
}

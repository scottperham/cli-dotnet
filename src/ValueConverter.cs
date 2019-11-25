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
                return true;
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

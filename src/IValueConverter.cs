using System;
using System.Reflection;

namespace cli_dotnet
{
    public interface IValueConverter
    {
        object GetValue(string value, Type type);
        object CreateDefaultValue(Type type);
    }
}

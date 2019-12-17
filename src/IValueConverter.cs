using System;

namespace cli_dotnet
{
    public interface IValueConverter
    {
        object GetValue(string value, Type type);
    }
}

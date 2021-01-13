using System;
using System.Reflection;

namespace cli_dotnet
{
    public interface IAttributeDecorator
    {
        void Decorate(CommandAttribute commandAttribute, Type globalOptionsType);
        void Decorate(VerbAttribute verbAttribute, Type globalOptionsType);
        void Decorate(GlobalOptionAttribute optionAttribute, PropertyInfo property);
    }
}

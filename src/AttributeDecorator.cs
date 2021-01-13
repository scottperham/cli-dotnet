using System;
using System.Reflection;

namespace cli_dotnet
{
    public class AttributeDecorator : IAttributeDecorator
    {
        private readonly ITypeHelper _typeHelper;
        private readonly IAttributeDecorator _attributeDecorator;

        public AttributeDecorator(ITypeHelper typeHelper, IAttributeDecorator attributeDecorator = null)
        {
            _typeHelper = typeHelper;
            _attributeDecorator = attributeDecorator ?? this;
        }

        void IAttributeDecorator.Decorate(VerbAttribute parentVerbAtt, Type globalOptionsType)
        {
            foreach (var member in _typeHelper.GetPropertiesAndMethods(parentVerbAtt.Instance))
            {
                if (member is PropertyInfo property && _typeHelper.TryGetVerbAttribute(property, out var childVerbAtt))
                {
                    childVerbAtt.Property = property;
                    childVerbAtt.ParentVerb = parentVerbAtt;
                    childVerbAtt.Instance = property.GetValue(parentVerbAtt.Instance);
                    _attributeDecorator.Decorate(childVerbAtt, globalOptionsType);
                    parentVerbAtt.Verbs[childVerbAtt.GetName()] = childVerbAtt;
                }
                else if (member is MethodInfo method && _typeHelper.TryGetCommandAttribute(method, out var commandAtt))
                {
                    commandAtt.ParentVerb = parentVerbAtt;
                    commandAtt.Method = method;
                    _attributeDecorator.Decorate(commandAtt, globalOptionsType);
                    parentVerbAtt.Commands[commandAtt.GetName()] = commandAtt;
                }
            }
        }

        void IAttributeDecorator.Decorate(CommandAttribute commandAttribute, Type globalOptionsType)
        {
            foreach (var parameter in commandAttribute.Method.GetParameters())
            {
                if (parameter.ParameterType == globalOptionsType)
                {
                    commandAttribute.GlobalOptionsParameter = parameter;
                    continue;
                }

                if (_typeHelper.TryGetValueAttribute(parameter, out var valueAtt))
                {
                    valueAtt.Parameter = parameter;
                    commandAttribute.Values.Add(valueAtt);

                    continue;
                }

                if (_typeHelper.TryGetOptionAttribute(parameter, out var optAtt))
                {
                    if (optAtt.ShortForm != '\0')
                    {
                        commandAttribute.Options[optAtt.ShortForm.ToString()] = optAtt;
                    }
                }
                else
                {
                    optAtt = new OptionAttribute();
                }

                optAtt.Parameter = parameter;

                if (optAtt.LongForm != null)
                {
                    commandAttribute.Options[optAtt.LongForm] = optAtt;
                }
            }
        }

        void IAttributeDecorator.Decorate(GlobalOptionAttribute optionAttribute, PropertyInfo property)
        {
            optionAttribute.Property = property;
        }
    }
}

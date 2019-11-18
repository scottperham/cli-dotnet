using System.Reflection;

namespace cli_dotnet
{
    public class AttributeDecorator : IAttributeDecorator
    {
        private readonly IAttributeDecorator _attributeDecorator;

        public AttributeDecorator(IAttributeDecorator attributeDecorator = null)
        {
            _attributeDecorator = attributeDecorator ?? this;
        }

        void IAttributeDecorator.Decorate(VerbAttribute parentVerbAtt)
        {
            foreach (var member in parentVerbAtt.Instance.GetType().GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.GetProperty | BindingFlags.InvokeMethod))
            {
                if (member is PropertyInfo property && property.TryGetCustomAttribute<VerbAttribute>(out var childVerbAtt))
                {
                    childVerbAtt.Property = property;
                    childVerbAtt.ParentVerb = parentVerbAtt;
                    childVerbAtt.Instance = property.GetValue(parentVerbAtt.Instance);
                    _attributeDecorator.Decorate(childVerbAtt);
                    parentVerbAtt.Verbs[childVerbAtt.GetName()] = childVerbAtt;
                }
                else if (member is MethodInfo method && method.TryGetCustomAttribute<CommandAttribute>(out var commandAtt))
                {
                    commandAtt.ParentVerb = parentVerbAtt;
                    commandAtt.Method = method;
                    _attributeDecorator.Decorate(commandAtt);
                    parentVerbAtt.Commands[commandAtt.GetName()] = commandAtt;
                }
            }
        }

        void IAttributeDecorator.Decorate(CommandAttribute commandAttribute)
        {
            foreach (var parameter in commandAttribute.Method.GetParameters())
            {
                if (parameter.TryGetCustomAttribute<ValueAttribute>(out var valueAtt))
                {
                    valueAtt.Parameter = parameter;
                    commandAttribute.Values.Add(valueAtt);

                    continue;
                }

                if (parameter.TryGetCustomAttribute<OptionAttribute>(out var optAtt))
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
                commandAttribute.Options[optAtt.GetName()] = optAtt;
            }
        }
    }
}

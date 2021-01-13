using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace cli_dotnet
{
    public class GlobalOptionsWrapper
    {
        public Type GlobalOptionType { get; set; }
        public object GlobalOptions { get; set; }
        public Dictionary<string, GlobalOptionAttribute> Options { get; set; }
    }

    public class CommandExecutor : ICommandExecutorImpl
    {
        private readonly ICommandParser _parser;
        private readonly ICommandExecutorOptions _options;
        private readonly IAttributeDecorator _attributeDecorator;
        private readonly IValueConverter _valueConverter;
        private readonly ICommandHelper _commandHelper;
        private readonly ITypeHelper _typeHelper;
        private readonly ICommandExecutorImpl _impl;

        public CommandExecutor(ICommandParser parser, ICommandExecutorOptions executorOptions, IAttributeDecorator attributeDecorator, IValueConverter valueConverter, ICommandHelper commandHelper, ITypeHelper typeHelper, ICommandExecutorImpl impl = null)
        {
            _parser = parser;
            _options = executorOptions;
            _attributeDecorator = attributeDecorator;
            _valueConverter = valueConverter;
            _commandHelper = commandHelper;
            _typeHelper = typeHelper;
            _impl = impl ?? this;
        }

        GlobalOptionsWrapper ICommandExecutorImpl.GetGlobalOptions<T>(T globalOptions)
        {
            var options = new Dictionary<string, GlobalOptionAttribute>();

            foreach(var property in _typeHelper.GetGlobalOptionProperties(globalOptions))
            {
                var optionAtt = property.GetCustomAttribute<GlobalOptionAttribute>();

                if (optionAtt == null)
                {
                    continue;
                }

                _attributeDecorator.Decorate(optionAtt, property);

                if (optionAtt.ShortForm != '\0')
                {
                    options[optionAtt.ShortForm.ToString()] = optionAtt;
                }

                if (optionAtt.LongForm != null)
                {
                    options[optionAtt.LongForm] = optionAtt;
                }
            }

            return new GlobalOptionsWrapper
            {
                GlobalOptionType = typeof(T),
                GlobalOptions = globalOptions,
                Options = options
            };
        }

        async public Task ExecuteAsync<TRoot, TGlobalOptions>(TRoot rootCommand, TGlobalOptions globalOptions)
        {
            var verbAtt = new VerbAttribute
            {
                IsRoot = true,
                Instance = rootCommand
            };

            var globalOptionsWrapper = _impl.GetGlobalOptions(globalOptions);

            _attributeDecorator.Decorate(verbAtt, typeof(TGlobalOptions));

            var commandParts = _parser.Parse().GetEnumerator();

            try
            {
                await _impl.ExecuteInternalAsync(verbAtt, commandParts, globalOptionsWrapper);
            }
            catch(BadCommandException bce)
            {
                Console.WriteLine(bce.BadCommand);

                if (bce.Command != null)
                {
                    _commandHelper.WriteCommandHelp(bce.Command, _options, globalOptionsWrapper);
                }
                else
                {
                    _commandHelper.WriteVerbHelp(bce.Verb, _options, globalOptionsWrapper);
                }
            }
        }

        async Task ICommandExecutorImpl.ExecuteInternalAsync(VerbAttribute verb, IEnumerator<CommandPart> commandParts, GlobalOptionsWrapper globalOptions)
        {
            if (!commandParts.MoveNext())
            {
                throw new BadCommandException(verb, "Malformed command");
            }

            if (commandParts.Current.IsArgument)
            {
                var key = _parser.GetString(commandParts.Current.Key);

                if (_impl.TrySetGlobalOption(key, commandParts.Current, globalOptions))
                {
                    await _impl.ExecuteInternalAsync(verb, commandParts, globalOptions);
                    return;
                }

                if (_commandHelper.TryShowHelpOrVersion(commandParts.Current, verb, key, _options, globalOptions))
                {
                    return;
                }

                throw new BadCommandException(verb, "Unexpected option");
            }

            var name = _parser.GetString(commandParts.Current.Key);

            if (verb.Verbs.TryGetValue(name, out var nextVerb))
            {
                await _impl.ExecuteInternalAsync(nextVerb, commandParts, globalOptions);

                return;
            }

            if (verb.Commands.TryGetValue(name, out var nextCommand))
            {
                await _impl.ExecuteCommandAsync(nextCommand, commandParts, globalOptions);
                return;
            }

            throw new BadCommandException(verb, name);
        }

        bool ICommandExecutorImpl.TryAddToLastParameter(ParameterInfo lastParameter, string value, SortedList<int, object> parameters)
        {
            object argValue;

            if (lastParameter?.ParameterType.IsArray != true)
            {
                return false;
            }

            Array existingArray;

            if (parameters.TryGetValue(lastParameter.Position, out var existingArgValue))
            {
                existingArray = (Array)existingArgValue;
            }
            else
            {
                existingArray = Array.CreateInstance(lastParameter.ParameterType.GetElementType(), 0);
            }

            argValue = _valueConverter.GetValue(value, lastParameter.ParameterType.GetElementType());

            var newArray = existingArray.ExtendAndAdd(argValue);
            parameters.AddOrUpdate(lastParameter.Position, newArray);

            return true;
        }

        bool ICommandExecutorImpl.TrySetGlobalOption(string key, CommandPart commandPart, GlobalOptionsWrapper globalOptions)
        {
            if (globalOptions?.Options == null || !globalOptions.Options.TryGetValue(key, out var optionAtt))
            {
                return false;
            }

            var value = _parser.GetString(commandPart.Value);

            var globalvalue = _valueConverter.GetValue(value, optionAtt.Property.PropertyType);

            optionAtt.Property.SetValue(globalOptions.GlobalOptions, globalvalue);

            return true;
        }

        ParameterInfo ICommandExecutorImpl.SetOptionParameter(string key, CommandPart commandPart, CommandAttribute command, SortedList<int, object> parameters)
        {
            if (!command.Options.TryGetValue(key, out var option))
            {
                throw new BadCommandException(command, $"Unknown option `{key}`");
            }

            var valueString = _parser.GetString(commandPart.Value);

            var value = _valueConverter.GetValue(valueString, option.Parameter.ParameterType);

            if (parameters.TryGetValue(option.Parameter.Position, out var existingValue))
            {
                if (!option.Parameter.ParameterType.IsArray)
                {
                    throw new BadCommandException(command, $"Option `{key}` specified too many times");
                }

                parameters[option.Parameter.Position] = ((Array)existingValue).ExtendAndAddArray((Array)value);
            }
            else
            {
                parameters.Add(option.Parameter.Position, value);
            }

            return option.Parameter;
        }

        ParameterInfo ICommandExecutorImpl.SetValueParameter(string key, CommandAttribute command, SortedList<int, object> parameters, ParameterInfo lastParameter)
        {
            if (_impl.TryAddToLastParameter(lastParameter, key, parameters))
            {
                return lastParameter;
            }

            var parameter = command.Values.Select(x => x.Parameter).FirstOrDefault(x => !parameters.ContainsKey(x.Position));

            if (parameter == null)
            {
                throw new BadCommandException(command, $"Too many values");
            }

            var value = _valueConverter.GetValue(key, parameter.ParameterType);

            parameters.Add(parameter.Position, value);

            return parameter;
        }

        async Task ICommandExecutorImpl.ExecuteCommandAsync(CommandAttribute command, IEnumerator<CommandPart> commandParts, GlobalOptionsWrapper globalOptions)
        {
            var parameters = new SortedList<int, object>();

            ParameterInfo lastParameter = null;

            while (commandParts.MoveNext())
            {
                var key = _parser.GetString(commandParts.Current.Key);

                if (commandParts.Current.IsArgument && _commandHelper.TryShowHelpOrVersion(commandParts.Current, command, key, _options, globalOptions))
                {
                    return;
                }

                if (commandParts.Current.IsArgument)
                {
                    if (_impl.TrySetGlobalOption(key, commandParts.Current, globalOptions))
                    {
                        continue;
                    }

                    lastParameter = _impl.SetOptionParameter(key, commandParts.Current, command, parameters);
                }
                else
                {
                    lastParameter = _impl.SetValueParameter(key, command, parameters, lastParameter);
                }
            }

            _impl.AddDefaultValues(command, parameters);

            if (globalOptions.GlobalOptions != null && command.GlobalOptionsParameter != null)
            {
                parameters[command.GlobalOptionsParameter.Position] = globalOptions.GlobalOptions;
            }

            await _impl.ExecuteActualCommandAsync(command, parameters);

            return;
        }

        void ICommandExecutorImpl.AddDefaultValues(CommandAttribute command, SortedList<int, object> parameters)
        {
            foreach (var parameter in command.Method.GetParameters())
            {
                if (!parameters.ContainsKey(parameter.Position))
                {
                    parameters.Add(parameter.Position, parameter.HasDefaultValue ? parameter.DefaultValue : _valueConverter.CreateDefaultValue(parameter.ParameterType));
                }
            }
        }

        async Task ICommandExecutorImpl.ExecuteActualCommandAsync(CommandAttribute command, SortedList<int, object> parameters)
        {
            var result = command.Method.Invoke(command.ParentVerb?.Instance, parameters.Values.ToArray());

            if (result is Task t)
            {
                await t;
            }
        }
    }
}

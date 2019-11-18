using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace cli_dotnet
{
    public class CommandExecutor : ICommandExecutorImpl
    {
        private readonly ICommandParser _parser;
        private readonly ICommandExecutorOptions _options;
        private readonly IAttributeDecorator _attributeDecorator;
        private readonly IValueConverter _valueConverter;
        private readonly ICommandHelper _commandHelper;
        private readonly ICommandExecutorImpl _impl;

        public CommandExecutor(ICommandParser parser, ICommandExecutorOptions executorOptions, IAttributeDecorator attributeDecorator, IValueConverter valueConverter, ICommandHelper commandHelper, ICommandExecutorImpl impl = null)
        {
            _parser = parser;
            _options = executorOptions;
            _attributeDecorator = attributeDecorator;
            _valueConverter = valueConverter;
            _commandHelper = commandHelper;
            _impl = impl ?? this;
        }

        async public Task ExecuteAsync<T>(T rootCommand)
        {
            var verbAtt = new VerbAttribute
            {
                IsRoot = true,
                Instance = rootCommand
            };

            _attributeDecorator.Decorate(verbAtt);

            var commandParts = _parser.Parse().GetEnumerator();

            try
            {
                await _impl.ExecuteInternalAsync(verbAtt, commandParts);
            }
            catch(BadCommandException bce)
            {
                Console.WriteLine(bce.BadCommand);

                if (bce.Command != null)
                {
                    _commandHelper.WriteCommandHelp(bce.Command, _options);
                }
                else
                {
                    _commandHelper.WriteVerbHelp(bce.Verb, _options);
                }
            }
        }

        async Task ICommandExecutorImpl.ExecuteInternalAsync(VerbAttribute verb, IEnumerator<CommandPart> commandParts)
        {
            if (!commandParts.MoveNext())
            {
                throw new BadCommandException(verb, "Malformed command");
            }

            if (commandParts.Current.IsArgument)
            {
                var key = _parser.GetString(commandParts.Current.Key);

                if (_commandHelper.TryShowHelp(commandParts.Current, verb, key, _options))
                {
                    return;
                }

                throw new BadCommandException(verb, "Unexpected option");
            }

            var name = _parser.GetString(commandParts.Current.Key);

            if (verb.Verbs.TryGetValue(name, out var nextVerb))
            {
                await _impl.ExecuteInternalAsync(nextVerb, commandParts);

                return;
            }

            if (verb.Commands.TryGetValue(name, out var nextCommand))
            {
                await _impl.ExecuteCommandAsync(nextCommand, commandParts);
                return;
            }

            throw new BadCommandException(verb, name);
        }

        async Task ICommandExecutorImpl.ExecuteCommandAsync(CommandAttribute command, IEnumerator<CommandPart> commandParts)
        {
            var parameters = new SortedList<int, object>();

            while (commandParts.MoveNext())
            {
                var key = _parser.GetString(commandParts.Current.Key);
                object argValue;
                int argPos;

                if (commandParts.Current.IsArgument)
                {
                    if (_commandHelper.TryShowHelp(commandParts.Current, command, key, _options))
                    {
                        return;
                    }

                    if (!command.Options.TryGetValue(key, out var option))
                    {
                        throw new BadCommandException(command, $"Unknown option {key}");
                    }

                    var value = _parser.GetString(commandParts.Current.Value);

                    argValue = _valueConverter.GetValue(value, option.Parameter, key);
                    argPos = option.Parameter.Position;
                }
                else
                {
                    var parameter = command.Values.Select(x => x.Parameter).FirstOrDefault(x => !parameters.ContainsKey(x.Position));

                    if (parameter == null)
                    {
                        throw new BadCommandException(command, $"Too many values");
                    }

                    argValue = _valueConverter.GetValue(key, parameter, key);
                    argPos = parameter.Position;
                }

                parameters.Add(argPos, argValue);
            }

            foreach(var parameter in command.Method.GetParameters())
            {
                if (!parameters.ContainsKey(parameter.Position))
                {
                    parameters.Add(parameter.Position, parameter.HasDefaultValue ? parameter.DefaultValue : _valueConverter.CreateDefaultValue(parameter.ParameterType));
                }
            }

            var result = command.Method.Invoke(command.ParentVerb.Instance, parameters.Values.ToArray());

            if (result is Task t)
            {
                await t;
            }

            return;
        }
    }
}

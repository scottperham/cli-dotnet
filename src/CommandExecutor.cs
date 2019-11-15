using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace cli_dotnet
{
    public class CommandExecutor
    {
        private readonly CommandParser _parser;
        private readonly CommandExecutorOptions _options;

        public CommandExecutor(string command, CommandExecutorOptions executorOptions = null)
        {
            _parser = new CommandParser(command);
            _options = executorOptions ?? new CommandExecutorOptions();
        }

        async public Task ExecuteAsync<T>(T rootCommand)
        {
            var verbAtt = new VerbAttribute
            {
                IsRoot = true,
                Instance = rootCommand
            };

            Decorate(verbAtt);

            var commandParts = _parser.Parse().GetEnumerator();

            try
            {
                await ExecuteInternalAsync(verbAtt, commandParts);
            }
            catch(BadCommandException bce)
            {
                Console.WriteLine(bce.BadCommand);

                if (bce.Command != null)
                {
                    CommandHelper.WriteCommandHelp(bce.Command, _options);
                }
                else
                {
                    CommandHelper.WriteVerbHelp(bce.Verb, _options);
                }
            }
        }

        async private Task ExecuteInternalAsync(VerbAttribute verb, IEnumerator<CommandPart> commandParts)
        {
            if (!commandParts.MoveNext())
            {
                throw new BadCommandException(verb, "Malformed command");
            }

            if (commandParts.Current.IsArgument)
            {
                var key = _parser.GetString(commandParts.Current.Key);

                if (CommandHelper.TryShowHelp(commandParts.Current, verb, key, _options))
                {
                    return;
                }

                throw new BadCommandException(verb, "Unexpected option");
            }

            var name = _parser.GetString(commandParts.Current.Key);

            if (verb.Verbs.TryGetValue(name, out var nextVerb))
            {
                await ExecuteInternalAsync(nextVerb, commandParts);

                return;
            }

            if (verb.Commands.TryGetValue(name, out var nextCommand))
            {
                await ExecuteCommandAsync(nextCommand, commandParts);
                return;
            }

            throw new BadCommandException(verb, name);
        }

        async private Task ExecuteCommandAsync(CommandAttribute command, IEnumerator<CommandPart> commandParts)
        {
            var parameters = new SortedList<int, object>();

            while (commandParts.MoveNext())
            {
                var key = _parser.GetString(commandParts.Current.Key);
                object argValue;
                int argPos;

                if (commandParts.Current.IsArgument)
                {
                    if (CommandHelper.TryShowHelp(commandParts.Current, command, key, _options))
                    {
                        return;
                    }

                    if (!command.Options.TryGetValue(key, out var option))
                    {
                        throw new BadCommandException(command, $"Unknown option {key}");
                    }

                    var value = _parser.GetString(commandParts.Current.Value);

                    argValue = GetValue(value, option.Parameter, key);
                    argPos = option.Parameter.Position;
                }
                else
                {
                    var parameter = command.Values.Select(x => x.Parameter).FirstOrDefault(x => !parameters.ContainsKey(x.Position));

                    if (parameter == null)
                    {
                        throw new BadCommandException(command, $"Too many values");
                    }

                    argValue = GetValue(key, parameter, key);
                    argPos = parameter.Position;
                }

                parameters.Add(argPos, argValue);
            }

            foreach(var parameter in command.Method.GetParameters())
            {
                if (!parameters.ContainsKey(parameter.Position))
                {
                    parameters.Add(parameter.Position, parameter.HasDefaultValue ? parameter.DefaultValue : Activator.CreateInstance(parameter.ParameterType));
                }
            }

            var result = command.Method.Invoke(command.ParentVerb.Instance, parameters.Values.ToArray());

            if (result is Task t)
            {
                await t;
            }

            return;
        }

        object GetValue(string value, ParameterInfo parameter, string name)
        {
            var type = parameter.ParameterType;

            if (type == typeof(bool))
            {
                return true;
            }

            if (type.HasElementType)
            {
                type = type.GetElementType();
            }

            try
            {
                return Convert.ChangeType(value, Type.GetTypeCode(type));
            }
            catch
            {
                throw new BadCommandException(parameter.ParameterType, $"Invalid value fro {name}");
            }
        }

        private void Decorate(VerbAttribute parentVerbAtt)
        {
            foreach (var member in parentVerbAtt.Instance.GetType().GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.GetProperty | BindingFlags.InvokeMethod))
            {
                if (member is PropertyInfo property && property.TryGetCustomAttribute<VerbAttribute>(out var childVerbAtt))
                {
                    childVerbAtt.Property = property;
                    childVerbAtt.ParentVerb = parentVerbAtt;
                    childVerbAtt.Instance = property.GetValue(parentVerbAtt.Instance);
                    Decorate(childVerbAtt);
                    parentVerbAtt.Verbs[childVerbAtt.GetName()] = childVerbAtt;
                }
                else if (member is MethodInfo method && method.TryGetCustomAttribute<CommandAttribute>(out var commandAtt))
                {
                    commandAtt.ParentVerb = parentVerbAtt;
                    commandAtt.Method = method;
                    Decorate(commandAtt);
                    parentVerbAtt.Commands[commandAtt.GetName()] = commandAtt;
                }
            }
        }

        private void Decorate(CommandAttribute commandAtt)
        {
            foreach (var parameter in commandAtt.Method.GetParameters())
            {
                if (parameter.TryGetCustomAttribute<ValueAttribute>(out var valueAtt))
                {
                    valueAtt.Parameter = parameter;
                    commandAtt.Values.Add(valueAtt);

                    continue;
                }

                if (parameter.TryGetCustomAttribute<OptionAttribute>(out var optAtt))
                {
                    if (optAtt.ShortForm != '\0')
                    {
                        commandAtt.Options[optAtt.ShortForm.ToString()] = optAtt;
                    }
                }
                else
                {
                    optAtt = new OptionAttribute();
                }

                optAtt.Parameter = parameter;
                commandAtt.Options[optAtt.GetName()] = optAtt;
            }
        }
    }
}

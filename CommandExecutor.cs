using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace CLI
{
    public class BadCommandException : Exception
    { 
        public BadCommandException(VerbAttribute verb, string badCommand)
        {
            Verb = verb;
            BadCommand = badCommand;
        }

        public BadCommandException(CommandAttribute command, string badCommand)
        {
            Command = command;
            BadCommand = badCommand;
        }

        public VerbAttribute Verb { get; }
        public CommandAttribute Command { get; }
        public string BadCommand { get; }
    }

    public class CommandExecutor
    {
        private CommandParser _parser;

        public CommandExecutor(string command)
        {
            _parser = new CommandParser(command);
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
                Console.WriteLine("Bad command");

                if (bce.Command != null)
                {
                    WriteCommandHelp(bce.Command);
                }
                else
                {
                    WriteVerbHelp(bce.Verb);
                }
            }
        }

        void WriteVerbHelp(VerbAttribute verb)
        {
            var sortedDictionary = new SortedDictionary<string, string>();

            GetVerbHelp(verb, "", sortedDictionary);

            foreach (var help in sortedDictionary)
            {
                Console.WriteLine($"{help.Key}\t\t\t{help.Value}");
            }
        }

        void WriteCommandHelp(CommandAttribute command)
        {
            Console.WriteLine("Command Syntax:");
            Console.Write($"\t{command.GetName()}");

            foreach(var value in command.Values)
            {
                Console.Write($" {{{value.Parameter.Name}}}");
            }

            if (command.Options.Count > 0)
            {
                Console.Write(" [Options]");
            }

            Console.WriteLine();

            if (command.Values.Count > 0)
            {
                Console.WriteLine("Values:");

                foreach(var value in command.Values)
                {
                    Console.WriteLine($"\t {value.GetName().PadRight(20, ' ')}{value.HelpText}");
                }
            }

            if (command.Options.Count == 0)
            {
                return;
            }

            Console.WriteLine("Options:");

            var written = new HashSet<OptionAttribute>();

            foreach(var option in command.Options)
            {
                Console.Write("\t");

                if (written.Contains(option.Value))
                {
                    continue;
                }

                string name = "";

                if (option.Value.ShortForm != '\0')
                {
                    name = " -" + option.Value.ShortForm;
                }

                if (option.Value.LongForm != null)
                {
                    name += " --" + option.Value.LongForm;
                }

                Console.WriteLine(name.PadRight(20, ' ') + option.Value.HelpText);

                written.Add(option.Value);
            }

            Console.WriteLine();
        }

        void GetVerbHelp(VerbAttribute verb, string prefix, SortedDictionary<string, string> help)
        {
            foreach(var innerVerb in verb.Verbs.Values)
            {
                GetVerbHelp(innerVerb, $"{prefix} {innerVerb.GetName()}", help);
            }

            foreach(var command in verb.Commands)
            {
                help.Add($"{prefix} {command.Key}", command.Value.HelpText);
            }
        }

        async private Task ExecuteInternalAsync(VerbAttribute verb, IEnumerator<CommandPart> commandParts)
        {
            if (!commandParts.MoveNext())
            {
                throw new BadCommandException(verb, "");
            }

            if (commandParts.Current.IsArgument)
            {
                var key = _parser.GetString(commandParts.Current.Key);

                if ((commandParts.Current.IsShortForm && key == "h") || (!commandParts.Current.IsShortForm && key.Equals("help", StringComparison.OrdinalIgnoreCase)))
                {
                    WriteVerbHelp(verb);
                    return;
                }

                throw new BadCommandException(verb, "");
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
                    if ((commandParts.Current.IsShortForm && key == "h") || (!commandParts.Current.IsShortForm && key.Equals("help", StringComparison.OrdinalIgnoreCase)))
                    {
                        WriteCommandHelp(command);
                        return;
                    }

                    if (!command.Options.TryGetValue(key, out var option))
                    {
                        throw new BadCommandException(command, key);
                    }

                    var value = _parser.GetString(commandParts.Current.Value);

                    argValue = GetValue(value, option.Parameter);
                    argPos = option.Parameter.Position;
                }
                else
                {

                    var parameter = command.Values.Select(x => x.Parameter).FirstOrDefault(x => !parameters.ContainsKey(x.Position));

                    if (parameter == null)
                    {
                        throw new BadCommandException(command, "");
                    }

                    argValue = GetValue(key, parameter);
                    argPos = parameter.Position;
                }

                parameters.Add(argPos, argValue);
            }

            foreach(var parameter in command.Method.GetParameters())
            {
                if (!parameters.ContainsKey(parameter.Position))
                {
                    if (!parameter.HasDefaultValue)
                    {
                        throw new BadCommandException(command, "");
                    }

                    parameters.Add(parameter.Position, parameter.DefaultValue);
                }
            }

            var result = command.Method.Invoke(command.ParentVerb.Instance, parameters.Values.ToArray());

            if (result is Task t)
            {
                await t;
            }

            return;
        }

        object GetValue(string value, ParameterInfo parameter)
        {
            if (parameter.ParameterType == typeof(bool))
            {
                return true;
            }

            return Convert.ChangeType(value, Type.GetTypeCode(parameter.ParameterType));
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

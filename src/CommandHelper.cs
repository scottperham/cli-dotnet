using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace cli_dotnet
{
    public class ConsoleCommandHelper : ICommandHelper
    {
        private readonly ICommandHelper _commandHelper;

        public ConsoleCommandHelper(ICommandHelper commandHelper = null)
        {
            _commandHelper ??= this;
        }

        bool ICommandHelper.TryShowHelp(CommandPart commandPart, CommandAttribute command, string key, ICommandExecutorOptions options)
        {
            if ((commandPart.IsShortForm && key[0] == options.HelpShortForm) || (!commandPart.IsShortForm && key.Equals(options.HelpLongForm, StringComparison.OrdinalIgnoreCase)))
            {
                _commandHelper.WriteCommandHelp(command, options);
                return true;
            }

            return false;
        }

        bool ICommandHelper.TryShowHelp(CommandPart commandPart, VerbAttribute verb, string key, ICommandExecutorOptions options)
        {
            if ((commandPart.IsShortForm && key[0] == options.HelpShortForm) || (!commandPart.IsShortForm && key.Equals(options.HelpLongForm, StringComparison.OrdinalIgnoreCase)))
            {
                _commandHelper.WriteVerbHelp(verb, options);
                return true;
            }

            return false;
        }

        void ICommandHelper.WriteVerbHelp(VerbAttribute verb, ICommandExecutorOptions options)
        {
            var sortedDictionary = new SortedDictionary<string, string>();

            _commandHelper.GetVerbHelp(verb, "", sortedDictionary);

            if (verb.IsRoot)
            {
                Console.WriteLine();
                Console.WriteLine("Usage:");

                Console.WriteLine($"    {Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly().Location)} [Command]"); 
            }

            Console.WriteLine();
            Console.WriteLine("Commands:");

            foreach (var help in sortedDictionary)
            {
                Console.WriteLine($"    {help.Key.PadRight(30, ' ')}{help.Value}");
            }

            Console.WriteLine();
            Console.WriteLine($"For help with command syntax, type `<command> --{options.HelpLongForm}` or `<command> -{options.HelpShortForm}`");
        }

        void ICommandHelper.WriteCommandHelp(CommandAttribute command, ICommandExecutorOptions options)
        {
            Console.WriteLine();
            Console.WriteLine("Command Syntax:");
            Console.Write($"    {command.GetName()}");

            foreach (var value in command.Values)
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
                Console.WriteLine();
                Console.WriteLine("Values:");

                foreach (var value in command.Values)
                {
                    Console.WriteLine($"    {value.GetName().PadRight(20, ' ')}{value.HelpText}");
                }
            }

            if (command.Options.Count == 0)
            {
                return;
            }

            Console.WriteLine();
            Console.WriteLine("Options:");

            var written = new HashSet<OptionAttribute>();

            foreach (var option in command.Options)
            {
                Console.Write("    ");

                if (written.Contains(option.Value))
                {
                    continue;
                }

                string name = "   ";

                if (option.Value.ShortForm != '\0')
                {
                    name = " -" + option.Value.ShortForm;
                }

                if (option.Value.LongForm != null)
                {
                    name += " --" + option.Value.LongForm + (option.Value.Parameter.ParameterType == typeof(bool) ? "" : "=<value>");
                }

                Console.WriteLine(name.PadRight(30, ' ') + option.Value.HelpText);

                written.Add(option.Value);
            }

            Console.WriteLine();
        }

        void ICommandHelper.GetVerbHelp(VerbAttribute verb, string prefix, SortedDictionary<string, string> help)
        {
            foreach (var innerVerb in verb.Verbs.Values)
            {
                _commandHelper.GetVerbHelp(innerVerb, $"{prefix} {innerVerb.GetName()}", help);
            }

            foreach (var command in verb.Commands)
            {
                help.Add($"{prefix} {command.Key}", command.Value.HelpText);
            }
        }
    }
}

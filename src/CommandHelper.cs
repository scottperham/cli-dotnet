using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace cli_dotnet
{
    public class ConsoleCommandHelper : ICommandHelper
    {
        private readonly ICommandHelper _commandHelper;

        public ConsoleCommandHelper(ICommandHelper commandHelper = null)
        {
            _commandHelper = commandHelper ?? this;
        }

        bool ICommandHelper.TryShowHelpOrVersion(CommandPart commandPart, CommandAttribute command, string key, ICommandExecutorOptions options, GlobalOptionsWrapper globalOptions)
        {
            if ((commandPart.IsShortForm && key[0] == options.VersionShortForm) || (!commandPart.IsShortForm && key.Equals(options.VersionLongForm, StringComparison.OrdinalIgnoreCase)))
            {
                _commandHelper.WriteVersion(options);
                return true;
            }

            if ((commandPart.IsShortForm && key[0] == options.HelpShortForm) || (!commandPart.IsShortForm && key.Equals(options.HelpLongForm, StringComparison.OrdinalIgnoreCase)))
            {
                _commandHelper.WriteCommandHelp(command, options, globalOptions);
                return true;
            }

            return false;
        }

        bool ICommandHelper.TryShowHelpOrVersion(CommandPart commandPart, VerbAttribute verb, string key, ICommandExecutorOptions options, GlobalOptionsWrapper globalOptions)
        {
            if ((commandPart.IsShortForm && key[0] == options.VersionShortForm) || (!commandPart.IsShortForm && key.Equals(options.VersionLongForm, StringComparison.OrdinalIgnoreCase)))
            {
                _commandHelper.WriteVersion(options);
                return true;
            }

            if ((commandPart.IsShortForm && key[0] == options.HelpShortForm) || (!commandPart.IsShortForm && key.Equals(options.HelpLongForm, StringComparison.OrdinalIgnoreCase)))
            {
                _commandHelper.WriteVerbHelp(verb, options, globalOptions);
                return true;
            }

            return false;
        }

        void ICommandHelper.WriteVerbHelp(VerbAttribute verb, ICommandExecutorOptions options, GlobalOptionsWrapper globalOptions)
        {
            var sortedDictionary = new SortedDictionary<string, string>();

            _commandHelper.GetVerbHelp(verb, "", sortedDictionary);

            if (verb.IsRoot)
            {
                WriteUsage(globalOptions, globalOptions.GlobalOptions != null, "COMMAND");

                WriteGlobalOptions(globalOptions);
            }

            Console.WriteLine();

            var commandGroups = verb.Commands.Select(x => x.Value).Distinct().Select(x => new
            {
                x.Category,
                Name = x.GetName(),
                x.HelpText
            }).Concat(verb.Verbs.Select(x => x.Value).Distinct().Select(x => new
            {
                x.Category,
                Name = x.GetName(),
                x.HelpText
            })).OrderBy(x => x.Name).GroupBy(x => x.Category);

            foreach(var commandGroup in commandGroups)
            {
                Console.WriteLine((commandGroup.Key ?? "Commands") + ":");

                foreach(var command in commandGroup)
                {
                    Console.CursorLeft = 2;
                    Console.Write(command.Name);

                    foreach (var line in GetLines(command.HelpText, 55))
                    {
                        Console.CursorLeft = 14;
                        Console.Write(line);
                    }

                    Console.WriteLine();
                }

                Console.WriteLine();
            }

            Console.WriteLine($"For help with command syntax, type `COMMAND --{options.HelpLongForm}` or `COMMAND -{options.HelpShortForm}`");
        }

        void ICommandHelper.WriteCommandHelp(CommandAttribute command, ICommandExecutorOptions options, GlobalOptionsWrapper globalOptions)
        {
            Console.Write($"Usage:  {command.GetName()}");

            if (options.ValuesFirst)
            {
                foreach (var value in command.Values)
                {
                    Console.Write($" {{{value.Parameter.Name}}}");
                }
            }

            if (command.Options.Count > 0)
            {
                Console.Write(" [Options]");
            }

            if (!options.ValuesFirst)
            {
                foreach (var value in command.Values)
                {
                    Console.Write($" {{{value.Parameter.Name}}}");
                }
            }

            Console.WriteLine();

            Console.WriteLine();
            Console.WriteLine(command.HelpText);

            if (command.Values.Count > 0)
            {
                Console.WriteLine();
                Console.WriteLine("Values:");

                foreach (var value in command.Values)
                {
                    Console.WriteLine($"    {value.GetName(),-20}{value.HelpText}");
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
                if (written.Contains(option.Value))
                {
                    continue;
                }

                Console.Write("    ");

                string name = "   ";

                if (option.Value.ShortForm != '\0')
                {
                    name = " -" + option.Value.ShortForm;
                }

                if (option.Value.LongForm != null)
                {
                    name += " --" + option.Value.LongForm + (option.Value.Parameter.ParameterType == typeof(bool) ? "" : "=<value>");
                }

                var helpText = option.Value.HelpText;

                if (option.Value.Parameter.ParameterType.IsArray)
                {
                    helpText = "(Array) " + helpText;
                }

                Console.WriteLine(name.PadRight(30, ' ') + helpText);

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

        public void WriteVersion(ICommandExecutorOptions options)
        {
            foreach(var version in options.VersionProvider.GetVersions())
            {
                Console.WriteLine(version);
            }
        }

        public void WriteUsage(GlobalOptionsWrapper globalOptions, bool options, string command)
        {
            Console.Write($"Usage: {Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly().Location)} ");

            if (globalOptions.GlobalOptions != null)
            {
                Console.Write("[OPTIONS] ");
            }

            Console.WriteLine("COMMAND");
            Console.WriteLine();
        }

        public void WriteGlobalOptions(GlobalOptionsWrapper globalOptions)
        {
            Console.WriteLine("Options:");

            var written = new HashSet<GlobalOptionAttribute>();

            foreach(var option in globalOptions.Options.Select(x => x.Value))
            {
                if (written.Contains(option))
                {
                    continue;
                }

                var type = GetTypeFromCLR(option.Property.PropertyType);

                WriteOption(option.ShortForm, option.LongForm, type, option.HelpText);

                written.Add(option);
            }
        }

        public string GetTypeFromCLR(Type type)
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64: return "int";
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal: return "float";
                case TypeCode.String:
                case TypeCode.Char: return "string";
            }

            if (type.IsArray)
            {
                return "list";
            }

            if (type.IsEnum)
            {
                return "string";
            }

            return null;
        }

        public void WriteOption(char shortForm, string longForm, string type, string helpText)
        {
            Console.CursorLeft = 2;

            if (shortForm != '\0')
            {
                Console.Write("-" + shortForm);

                WriteIf(longForm != null, ",");
            }

            Console.CursorLeft = 6;

            var nameLen = 0;

            if (longForm != null)
            {
                nameLen += longForm.Length;
                Console.Write("--" + longForm);
            }

            if (type != null)
            {
                nameLen += type.Length;
                Console.Write(" " + type);
            }

            var lines = GetLines(helpText, 46);

            if (lines.Length == 0)
            {
                Console.WriteLine();
            }
            else
            {
                if (nameLen > 17)
                {
                    Console.WriteLine();
                }

                foreach (var line in lines)
                {
                    Console.CursorLeft = 33;

                    Console.Write(line);
                    Console.WriteLine();
                }
            }
        }

        public string[] GetLines(string text, int maxLen)
        {
            if (text == null)
            {
                return new string[0];
            }

            var charCount = 0;

            return text.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .GroupBy(w => (charCount += w.Length + 1) / maxLen)
                .Select(g => string.Join(" ", g))
                .ToArray();
        }

        public void WriteIf(bool condition, string toWrite)
        {
            if (condition)
            {
                Console.Write(toWrite);
            }
        }
    }
}

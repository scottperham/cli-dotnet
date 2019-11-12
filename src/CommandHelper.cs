using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace cli_dotnet
{
    public class CommandHelper
    {
        public static void WriteVerbHelp(VerbAttribute verb)
        {
            var sortedDictionary = new SortedDictionary<string, string>();

            GetVerbHelp(verb, "", sortedDictionary);

            if (verb.IsRoot)
            {
                Console.WriteLine("Usage:");

                Console.WriteLine($"    {Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly().Location)} [Command]");
            }

            Console.WriteLine("Commands:");

            foreach (var help in sortedDictionary)
            {
                Console.WriteLine($"    {help.Key.PadRight(30, ' ')}{help.Value}");
            }

            Console.WriteLine("For help with command syntax, type `<command> --help` or `<command> -h`");
        }

        public static void WriteCommandHelp(CommandAttribute command)
        {
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

        static void GetVerbHelp(VerbAttribute verb, string prefix, SortedDictionary<string, string> help)
        {
            foreach (var innerVerb in verb.Verbs.Values)
            {
                GetVerbHelp(innerVerb, $"{prefix} {innerVerb.GetName()}", help);
            }

            foreach (var command in verb.Commands)
            {
                help.Add($"{prefix} {command.Key}", command.Value.HelpText);
            }
        }
    }
}

using System;
using System.Threading.Tasks;

namespace cli_dotnet
{
    public static class Cli
    {
        /// <summary>
        /// Parses and executes a command.
        /// </summary>
        /// <param name="rootCommand">The object containing the first <see cref="VerbAttribute"/>s and <see cref="CommandAttribute"/>s.</param>
        /// <param name="command">Optional command string, if this is left null, the process arguments will be used instead.</param>
        /// <param name="commandExecutorOptions">Command executor options, such as the short form value for a help argument.</param>
        /// <returns></returns>
        public static Task ExecuteAsync<T>(T rootCommand, string command = null, ICommandExecutorOptions commandExecutorOptions = null)
        {
            var parser = new CommandParser(command ?? GetCommandLine());
            var options = commandExecutorOptions ?? CommandExecutorOptions.Default;
            var attributeHelper = new TypeHelper();
            var attributeDecorator = new AttributeDecorator(attributeHelper);
            var valueConverter = new ValueConverter();
            var commandHelper = new ConsoleCommandHelper();
            var commandExecutor = new CommandExecutor(parser, options, attributeDecorator, valueConverter, commandHelper);

            return commandExecutor.ExecuteAsync(rootCommand);
        }

        static string GetCommandLine()
        {
            var exe = Environment.GetCommandLineArgs()[0];
            var rawCmd = Environment.CommandLine;

            return rawCmd.Remove(rawCmd.IndexOf(exe), exe.Length).TrimStart('"').Trim();
        }
    }
}

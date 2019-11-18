using System;
using System.Threading.Tasks;

namespace cli_dotnet
{
    public static class Cli
    {
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

namespace cli_dotnet
{
    public class CommandExecutorOptions : ICommandExecutorOptions
    {
        public char HelpShortForm { get; } = 'h';
        public string HelpLongForm { get; } = "help";

        public static ICommandExecutorOptions Default => new CommandExecutorOptions();
    }
}

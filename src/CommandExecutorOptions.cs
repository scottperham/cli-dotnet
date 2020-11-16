namespace cli_dotnet
{
    public class CommandExecutorOptions : ICommandExecutorOptions
    {
        public char HelpShortForm { get; } = 'h';
        public string HelpLongForm { get; } = "help";

        public char VersionShortForm { get; } = 'v';
        public string VersionLongForm { get; } = "version";

        public IVersionProvider VersionProvider { get; } = new AssemblyVersionProvider();

        public static ICommandExecutorOptions Default => new CommandExecutorOptions();
    }
}

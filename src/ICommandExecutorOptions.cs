namespace cli_dotnet
{
    public interface ICommandExecutorOptions
    {
        char HelpShortForm { get; }
        string HelpLongForm { get; }

        char VersionShortForm { get; }
        string VersionLongForm { get; }

        IVersionProvider VersionProvider { get; }
    }
}

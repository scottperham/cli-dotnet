namespace cli_dotnet
{
    public interface ICommandExecutorOptions
    {
        char HelpShortForm { get; }
        string HelpLongForm { get; }
    }
}

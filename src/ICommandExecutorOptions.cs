using System;

namespace cli_dotnet
{
    public interface ICommandExecutorOptions
    {
        char HelpShortForm { get; }
        string HelpLongForm { get; }

        char VersionShortForm { get; }
        string VersionLongForm { get; }

        bool ValuesFirst { get; set; }

        IVersionProvider VersionProvider { get; }

        void SetGlobalOptions<T>();
    }
}

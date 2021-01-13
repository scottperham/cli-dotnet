using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace cli_dotnet
{
    public class CommandExecutorOptions : ICommandExecutorOptions
    {
        public char HelpShortForm { get; } = 'h';
        public string HelpLongForm { get; } = "help";

        public char VersionShortForm { get; } = 'v';
        public string VersionLongForm { get; } = "version";

        public bool ValuesFirst { get; set; } = true;

        public IVersionProvider VersionProvider { get; } = new AssemblyVersionProvider();

        public static ICommandExecutorOptions Default => new CommandExecutorOptions();

        public void SetGlobalOptions<T>()
        {
            var options = new Dictionary<string, GlobalOptionAttribute>();

            foreach(var property in typeof(T).GetProperties())
            {
                var optAtt = property.GetCustomAttribute<GlobalOptionAttribute>();

                if (optAtt == null)
                {
                    continue;
                }

                optAtt.Property = property;

                if (optAtt.ShortForm != '\0')
                {
                    options[optAtt.ShortForm.ToString()] = optAtt;
                }

                if (optAtt.LongForm != null)
                {
                    options[optAtt.LongForm] = optAtt;
                }
            }
        }
    }
}

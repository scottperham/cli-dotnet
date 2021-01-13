using cli_dotnet;
using System.Reflection;

namespace docker_cli
{
    public class Builder
    {
        [Command(name: "build", "Build an image from a Dockerfile")]
        public void Build()
        {
            
        }

        [Command(name: "prune", "Remove build cache")]
        public void Prune(
            GlobalOptions globalOptions,
            [Option(shortForm:'a', helpText: "Remove all unused build cache, not just dangling ones")] string all,
            [Option(helpText: "Provider filter values (e.g. 'until=24h')")] string filter,
            [Option(shortForm: 'f', helpText: "Do not prompt for confirmation")] bool force,
            [Option(longForm: "keep-storage", helpText: "Amount of disk space to keep for cache")] int keepStorage)
        {
            DebugWriter.WriteMethodCall(MethodBase.GetCurrentMethod(), globalOptions, all, filter, force, keepStorage);
        }
    }
}

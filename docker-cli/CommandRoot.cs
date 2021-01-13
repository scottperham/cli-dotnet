using cli_dotnet;
using System.Reflection;

namespace docker_cli
{
    public class CommandRoot
    {
        [Command]
        public void Test(GlobalOptions globalOptions, [Value] string test)
        {
            DebugWriter.Write(globalOptions);
            DebugWriter.Write(nameof(test), test);
        }

        [Verb(name: "builder", helpText: "Manage builds", category: "Management Commands")]
        public Builder Builder => new Builder();

        [Command(name: "attach", "Attach local standard input, output, and error streams to a running container")]
        public void Attach(
            GlobalOptions globalOptions,
            [Value]string[] container,
            [Option(longForm: "detach-keys", helpText:"Override the key request for detacthing a container")] string detachKeys, 
            [Option(longForm: "no-stdin", helpText:"Do not attach STDIN")] bool noStdIn, 
            [Option(longForm:"sig-proxy", helpText:"Proxy all received signals to the process")] bool sigProxy)
        {
            DebugWriter.WriteMethodCall(MethodBase.GetCurrentMethod(), globalOptions, container, detachKeys, noStdIn, sigProxy);
        }
    }
}

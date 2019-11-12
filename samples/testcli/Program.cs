using cli_dotnet;
using System;
using System.Threading.Tasks;

namespace testcli
{
    class Program
    {
        async static Task Main(string[] args)
        {
            while (true)
            {
                Console.Write("> ");

                var commandExecutor = new CommandExecutor(Console.ReadLine());

                await commandExecutor.ExecuteAsync(new CommandRoot());
            }
        }
    }

    public class CommandRoot
    {
        [Verb]
        public CreateVerb Create { get; } = new CreateVerb();

        [Command]
        public Task Connect(
            [Value(helpText: "The name of the host")]
            string host,
            [Value(helpText: "The port to connect to")]
            ushort port)
        {
            Console.WriteLine($"Connecting to {host}:{port}");
            return Task.CompletedTask;
        }
    }

    public class CreateVerb
    {
        [Command("Queue")]
        public Task CreateQueue(
            [Value(helpText: "The name of the queue")]
            string name,
            [Value(helpText: "The other name of the queue")]
            string somethingElse,
            [Option('d', "durable", "Survives restarts")]
            bool durable = false
        )
        {
            Console.WriteLine($"Creating a {(durable ? "" : "non-")}durable queue called {name}");
            Console.WriteLine(somethingElse);
            return Task.CompletedTask;
        }
    }
}

using cli_dotnet;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace testcli
{
    class Program
    {
        async static Task Main(string[] args)
        {
            var state = new State();

            if (args.Length == 0)
            {
                while (true)
                {
                    Console.Write($"{state.CurrentContext?.Username ?? "root"}> ");

                    var command = Console.ReadLine();

                    if (string.IsNullOrWhiteSpace(command))
                    {
                        continue;
                    }

                    if (command.Equals("exit", StringComparison.OrdinalIgnoreCase))
                    {
                        break;
                    }

                    await ExecuteAsync(command, state);

                    Console.WriteLine();
                }
            }
            else
            {
                state.Users.Add("test", new State.User { Username = "test", Password = "test" });
                state.Users.Add("test2", new State.User { Username = "test2", Password = "test2" });
                state.Users.Add("test3", new State.User { Username = "test3", Password = "test3" });
                state.Users.Add("test4", new State.User { Username = "test4", Password = "test4" });

                await ExecuteAsync(null, state);
            }
        }
        
        async static Task ExecuteAsync(string command, State state)
        {
            try
            {
                await Cli.ExecuteAsync(new CommandRoot(state), command, new GlobalOptions());
            }
            catch(Exception ex)
            {
                var oldColor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.GetBaseException().Message);
                Console.ForegroundColor = oldColor;
            }
        }
    }

    public class GlobalOptions
    { 
        [GlobalOption]
        public string LogLevel { get; set; }
    }
}

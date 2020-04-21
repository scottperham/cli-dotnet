using cli_dotnet;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace testtool
{
    class Program
    {
        async static Task Main(string[] args)
        {
            if (args.Length == 0)
            {
                while (true)
                {
                    Console.Write($"> ");

                    var command = Console.ReadLine();

                    if (command.Equals("exit", StringComparison.OrdinalIgnoreCase))
                    {
                        break;
                    }

                    await ExecuteAsync(command);

                    Console.WriteLine();
                }
            }
            else
            {
                await ExecuteAsync(null);
            }
        }

        async static Task ExecuteAsync(string command)
        {
            try
            {
                await Cli.ExecuteAsync(typeof(Program).GetMethod(nameof(Run), BindingFlags.Static | BindingFlags.NonPublic), command);
            }
            catch (Exception ex)
            {
                var oldColor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.GetBaseException().Message);
                Console.ForegroundColor = oldColor;
            }
        }

        [Command("tool", helpText: "Runs the tool")]
        static Task Run(
            [Value] string[] values,
            [Option('a')] bool flag
            )
        {
            Console.WriteLine("Running tool with:");
            foreach(var value in values)
            {
                Console.WriteLine("    " + value);
            }

            if (flag)
            {
                Console.WriteLine("    and flag");
            }

            return Task.CompletedTask;
        }
    }
}

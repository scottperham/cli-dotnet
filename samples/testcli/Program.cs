using cli_dotnet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace testcli
{
    class Program
    {
        async static Task Main(string[] args)
        {
            var state = new State();

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

                try
                {
                    await ExecuteAsync(command, state);
                }
                catch(Exception ex)
                {
                    var oldColor = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(ex.GetBaseException().Message);
                    Console.ForegroundColor = oldColor;
                }

                Console.WriteLine();
            }
        }

        async static Task ExecuteAsync(string command, State state)
        {
            var commandExecutor = new CommandExecutor(command);

            await commandExecutor.ExecuteAsync(new CommandRoot(state));
        }
    }

    public class State
    {
        public class User
        {
            public string Username { get; set; }
            public string Password { get; set; }
            public bool CanLogin { get; set; }
        }

        public User CurrentContext = null;
        public Dictionary<string, User> Users { get; } = new Dictionary<string, User>(StringComparer.OrdinalIgnoreCase);
    }

    public class CommandRoot
    {
        private readonly State _state;

        public CommandRoot(State state)
        {
            User = new UserActions(state);
            _state = state;
        }

        [Verb]
        public UserActions User { get; }

        [Command(helpText:"Logs in a user")]
        public Task Login(
            [Value(helpText:"The username of the user to login")]string username, 
            [Value(helpText: "The password of the user to login")]string password)
        {
            if (!_state.Users.TryGetValue(username, out var user) || user.Password != password)
            {
                throw new Exception("Unable to login, bad username or password");
            }

            if (!user.CanLogin)
            {
                throw new Exception("User not allowed to login");
            }

            Console.WriteLine("Login successful");

            _state.CurrentContext = user;

            return Task.CompletedTask;
        }

        [Command(helpText:"Logs out the current user")]
        public Task Logout()
        {
            _state.CurrentContext = null;
            return Task.CompletedTask;
        }
    }

    public class UserActions
    {
        private readonly State _state;

        public UserActions(State state)
        {
            _state = state;
        }

        [Command(helpText:"Creates a new user")]
        public Task Create(
            [Value] string username, 
            [Value] string password,
            [Option(longForm:"preventlogin")] bool preventLogin = false)
        {
            if (_state.Users.ContainsKey(username))
            {
                throw new Exception("User already exists with that username");
            }

            _state.Users[username] = new State.User
            {
                Username = username,
                Password = password,
                CanLogin = !preventLogin
            };

            Console.WriteLine("User created");

            return Task.CompletedTask;
        }

        [Command(helpText:"Lists users")]
        public Task List(
            [Option('a', "all", "Include users who aren't allowed to login")]bool showAll,
            [Option('d', "desc", "Sort users in decending order")]bool descending)
        {
            var users = _state.Users.Values.AsEnumerable();

            if (descending)
            {
                users = users.OrderByDescending(x => x.Username);
            }
            else
            {
                users = users.OrderBy(x => x.Username);
            }

            foreach(var user in users)
            {
                if (!showAll && !user.CanLogin)
                {
                    continue;
                }

                Console.WriteLine(user.Username);
            }

            return Task.CompletedTask;
        }

        [Command(helpText:"Removes a user")]
        public Task Remove(
            [Value] string username)
        {
            if (_state.CurrentContext?.Username.Equals(username, StringComparison.OrdinalIgnoreCase) == true)
            {
                throw new Exception("Unable to remove current user");
            }

            _state.Users.Remove(username);

            Console.WriteLine("User removed");

            return Task.CompletedTask;
        }
    }
}

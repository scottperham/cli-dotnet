using cli_dotnet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace testcli
{
    public class UserActions
    {
        private readonly State _state;

        public UserActions(State state)
        {
            _state = state;
        }

        [Command]
        public Task Permissions([Value] string username, [Option('p')] string[] perms, [Option('o', noLongForm: true)] bool overwrite)
        {
            Console.WriteLine($"Setting permissions for {username}");

            Console.WriteLine("Overwrite: " + overwrite);

            foreach(var perm in perms)
            {
                Console.WriteLine(perm);
            }

            return Task.CompletedTask;
        }

        [LocalizedCommand(helpTextKey:"CREATE NEW USER KEY")]
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
            [Option('d', "desc", "Sort users in decending order")]bool descending,
            [Option('s')] bool singleLine)
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

            var first = true;

            foreach(var user in users)
            {
                if (!showAll && !user.CanLogin)
                {
                    continue;
                }

                if (!first)
                {
                    Console.Write(singleLine ? ' ' : '\n');
                }

                Console.Write(user.Username);

                first = false;
            }

            return Task.CompletedTask;
        }

        [Command(helpText:"Removes a user")]
        public Task Remove(
            [Value] string[] username)
        {
            foreach (var user in username)
            {
                if (_state.CurrentContext?.Username.Equals(user, StringComparison.OrdinalIgnoreCase) == true)
                {
                    throw new Exception("Unable to remove current user");
                }

                if (!_state.Users.Remove(user))
                {
                    Console.WriteLine($"Cannot find user {user}");
                }
                else
                {
                    Console.WriteLine($"User {user} removed");
                }
            }
            return Task.CompletedTask;
        }
    }
}

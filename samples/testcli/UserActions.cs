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

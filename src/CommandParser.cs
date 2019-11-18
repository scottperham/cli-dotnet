using System;
using System.Collections.Generic;
using System.Linq;

namespace cli_dotnet
{
    public class CommandParser : ICommandParser, ICommandParserImplementation
    {
        readonly ICommandParserImplementation _impl;

        public CommandParser(string command, ICommandParserImplementation impl = null)
        {
            command = command ?? throw new ArgumentNullException(nameof(command));

            //Used for unit testing, the default behaviour should be to use
            //_this_ as the implementation
            _impl = impl ?? this;

            _impl.Position = -1;
            _impl.Command = command;
        }

        int ICommandParserImplementation.Position { get; set; }
        string ICommandParserImplementation.Command{ get; set; }

        string ICommandParser.GetString(StringReference reference)
        {
            if (reference.Start < 0 || reference.Start + reference.Length > _impl.Command.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(reference));
            }

            return _impl.Command.Substring(reference.Start, reference.Length);
        }

        char ICommandParserImplementation.Peek()
        {
            return _impl.Position >= _impl.Command.Length - 1 ? '\0' : _impl.Command[_impl.Position + 1];
        }

        void ICommandParserImplementation.Consume()
        {
            _impl.Position = Math.Min(_impl.Position + 1, _impl.Command.Length);
        }

        bool ICommandParserImplementation.ConsumeIf(char ch)
        {
            if (_impl.Peek() == ch)
            {
                ++_impl.Position;
                return true;
            }

            return false;
        }

        StringReference ICommandParserImplementation.ScanToAny(params char[] scanTo)
        {
            int start = -1;
            char ch;
            while ((ch = _impl.Peek()) != '\0')
            {
                if (scanTo.Contains(ch))
                {
                    break;
                }

                _impl.Consume();

                if (start == -1)
                {
                    start = _impl.Position;
                }
            }

            return new StringReference(start, _impl.Position + 1 - start);
        }

        StringReference ICommandParserImplementation.ScanTo(char ch, char escape = '\0')
        {
            char next;
            char prev = '\0';
            var start = -1;

            while((next = _impl.Peek()) != '\0')
            {
                if (next == ch && (escape == '\0' || escape != prev))
                {
                    break;
                }

                prev = next;

                _impl.Consume();

                if (start == -1)
                {
                    start = _impl.Position;
                }
            }

            return new StringReference(start, _impl.Position + 1 - start);
        }

        public IEnumerable<CommandPart> Parse()
        {
            while (_impl.TryGetCommandPart(out var commandPart))
            {
                if (commandPart.IsShortForm)
                {
                    if (commandPart.Key.Length > 1 && !commandPart.Value.IsEmpty())
                    {
                        throw new BadCommandException("Cannot assign value to multi short form");
                    }

                    for (var i = 0; i < commandPart.Key.Length; i++)
                    {
                        yield return new CommandPart
                        {
                            IsShortForm = true,
                            IsArgument = true,
                            Key = new StringReference
                            {
                                Start = commandPart.Key.Start + i,
                                Length = 1
                            },
                            Value = commandPart.Key.Length == 1 ? commandPart.Value : new StringReference()
                        };
                    }
                }
                else 
                {
                    yield return commandPart;
                }
            }
        }

        void ICommandParserImplementation.ConsumeWhitespace()
        {
            char ch;
            while ((ch = _impl.Peek()) != '\0')
            {
                if (char.IsWhiteSpace(ch))
                {
                    _impl.Consume();
                    continue;
                }

                break;
            }
        }

        CommandPart ICommandParserImplementation.GetLongFormArgument()
        {
            //Assume no leading whitepace

            var key = _impl.ScanToAny('=', ' ');
            StringReference value = default;    

            if (_impl.Peek() == '=')
            {
                _impl.Consume();

                value = _impl.GetValueReference(true);
            }

            return new CommandPart
            {
                IsArgument = true,
                Key = key,
                Value = value
            };
        }

        CommandPart ICommandParserImplementation.GetShortFormArgument()
        {
            //Assume no leading whitespace

            var key = _impl.GetValueReference(false);
            StringReference value = default;

            if (key.Length == 1) // might have value
            {
                //Consume space
                _impl.ConsumeWhitespace();

                var ch = _impl.Peek();

                if (ch != '-' && ch != '\0')
                {
                    value = _impl.GetValueReference(true);
                }
            }

            return new CommandPart
            {
                IsArgument = true,
                IsShortForm = true,
                Key = key,
                Value = value
            };
        }

        CommandPart ICommandParserImplementation.GetValue()
        {
            return new CommandPart
            {
                Key = _impl.GetValueReference(true)
            };
        }

        bool ICommandParserImplementation.TryGetCommandPart(out CommandPart commandPart)
        {
            commandPart = default;

            _impl.ConsumeWhitespace();

            if (_impl.Peek() == '\0')
            {
                return false;
            }

            if (_impl.ConsumeIf('-'))
            {
                if (_impl.ConsumeIf('-'))
                {
                    commandPart = _impl.GetLongFormArgument();
                }
                else
                {
                    commandPart = _impl.GetShortFormArgument();
                }
            }
            else
            {
                commandPart = _impl.GetValue();
            }

            return true;
        }

        StringReference ICommandParserImplementation.GetValueReference(bool allowQuotes)
        {
            StringReference value;
            var ch = _impl.Peek();

            if (allowQuotes && (ch == '"' || ch == '\''))
            {
                _impl.Consume();

                value = _impl.ScanTo(ch, '\\');

                _impl.Consume();
            }
            else
            {
                value = _impl.ScanTo(' ');
            }

            return value;
        }
    }
}

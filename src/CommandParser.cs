using System;
using System.Collections.Generic;
using System.Linq;

namespace cli_dotnet
{
    public class CommandParser
    {
        readonly string _command;
        int _position = -1;

        public CommandParser(string command)
        {
            _command = command;
        }

        public string GetString(StringReference reference)
        {
            return _command.Substring(reference.Start, reference.Length);
        }

        char Peek()
        {
            return _position >= _command.Length - 1 ? '\0' : _command[_position + 1];
        }

        void Consume()
        {
            _position = Math.Min(_position + 1, _command.Length);
        }

        bool ConsumeIf(char ch)
        {
            if (Peek() == ch)
            {
                ++_position;
                return true;
            }

            return false;
        }

        StringReference ScanTo(char ch, char escape = '\0', bool consumeFinal = true, params char[] stopIf)
        {
            var start = 0;
            var end = 0;
            var first = true;
            var prev = '\0';
            char next;

            while ((next = Peek()) != '\0')
            {
                if ((next == ch && (escape == '\0' || escape != prev)) || stopIf.Contains(next))
                {
                    if (consumeFinal)
                    {
                        Consume();
                    }
                    end = _position + (consumeFinal ? 0 : 1);
                    break;
                }

                Consume();

                if (first)
                {
                    start = _position;
                    first = false;
                }

                prev = next;
            }
            
            if (!first && end == 0)
            {
                if (consumeFinal)
                {
                    Consume();
                }
                end = _position + (consumeFinal ? 0 : 1);
            }

            return new StringReference
            {
                Start = start,
                Length = end - start
            };
        }

        public IEnumerable<CommandPart> Parse()
        {
            while (TryGetCommandPart(out var commandPart))
            {
                if (commandPart.IsShortForm)
                {
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

        bool TryGetCommandPart(out CommandPart commandPart)
        {
            var longForm = false;
            var consumeLast = false;

            bool isArg;
            if (isArg = ConsumeIf('-'))
            {
                longForm = ConsumeIf('-');
            }

            StringReference value = default;
            StringReference key = default;

            if (isArg)
            {
                consumeLast = true;

                key = ScanTo(longForm ? '=' : ' ', '\0', false, ' ', '\0');

                if (longForm || (!longForm && key.Length == 1))
                {
                    var ch = Peek();

                    if ((longForm && ch == '=') || (!longForm && ch == ' '))
                    {
                        Consume();

                        consumeLast = false;

                        ch = Peek();

                        if (longForm || ch != '-')
                        {
                            if (ch == '"' || ch == '\'')
                            {
                                Consume();

                                value = ScanTo(ch, '\\');

                                ConsumeIf(' ');
                            }
                            else
                            {
                                value = ScanTo(' ');
                            }
                        }
                    }
                }
            }
            else
            {
                var ch = Peek();

                if (ch == '"' || ch == '\'')
                {
                    Consume();

                    key = ScanTo(ch, '\\');

                    ConsumeIf(' ');
                }
                else
                {
                    key = ScanTo(' ');
                }
            }

            if (consumeLast)
            {
                Consume();
            }

            commandPart = new CommandPart
            {
                IsArgument = isArg,
                IsShortForm = isArg && !longForm,
                Key = key,
                Value = value
            };

            return !commandPart.Key.IsEmpty();
        }
    }
}

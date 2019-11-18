using FluentAssertions;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace cli_dotnet.test.unit
{
    public class ParserTests
    {
        public CommandParser CreateSut(string command)
        {
            return new CommandParser(command);
        }

        [Fact]
        public void EmptyCommand_ReturnsNoCommandParts()
        {
            var command = "";

            var sut = CreateSut(command);

            var commandParts = sut.Parse();

            commandParts.Should().BeEmpty();
        }

        public struct TestCommandPart
        {
            public bool IsLongForm;
            public bool IsShortForm;
            public string Key;
            public string Value;
            public char QuoteChar;

            public CommandPart GetCommandPart(ref int pos)
            {
                var part = new CommandPart();

                if (IsShortForm || IsLongForm)
                {
                    ++pos;
                    part.IsArgument = true;

                    if (IsLongForm)
                    {
                        ++pos;
                    }
                    else
                    {
                        part.IsShortForm = true;
                    }

                    part.Key = new StringReference
                    {
                        Start = pos,
                        Length = Key.Length
                    };

                    pos += Key.Length;

                    if (Value != default)
                    {
                        ++pos; // Space or equals
                        pos += QuoteChar != default ? 1 : 0;

                        part.Value = new StringReference
                        {
                            Start = pos,
                            Length = Value.Length
                        };

                        pos += Value.Length;

                        pos += QuoteChar != default ? 1 : 0;
                    }
                }
                else
                {
                    pos += QuoteChar != default ? 1 : 0;

                    part.Key = new StringReference
                    {
                        Start = pos,
                        Length = Value.Length
                    };

                    pos += Value.Length;

                    pos += QuoteChar != default ? 1 : 0;
                }

                ++pos;

                return part;
            }

            public string GetCommand()
            {
                var command = new StringBuilder();

                if (IsShortForm || IsLongForm)
                {
                    command.Append('-');

                    if (IsLongForm)
                    {
                        command.Append('-');
                    }

                    command.Append(Key);

                    if (Value != default)
                    {
                        command.Append(IsLongForm ? '=' : ' ');
                    }
                }

                if (Value != default && QuoteChar != default)
                {
                    command.Append(QuoteChar);
                }

                if (Value != default)
                {
                    command.Append(Value);
                }

                if (Value != default && QuoteChar != default)
                {
                    command.Append(QuoteChar);
                }

                return command.ToString();
            }
        }

        (string, IEnumerable<CommandPart>) TestSetup(params IEnumerable<TestCommandPart>[] parts)
        {
            var commands = new List<string>();
            var commandParts = new List<CommandPart>();
            var pos = 0;

            foreach(var part in parts)
            {
                foreach (var subPart in part)
                {
                    commands.Add(subPart.GetCommand());
                    commandParts.Add(subPart.GetCommandPart(ref pos));
                }
            }

            return (string.Join(' ', commands), commandParts);
        }

        TestCommandPart TCP(string key, bool sf, bool lf, string value = default, char quoteChar = default)
        {
            return new TestCommandPart
            {
                IsShortForm = sf,
                IsLongForm = lf,
                QuoteChar = quoteChar,
                Key = key,
                Value = value
            };
        }

        TestCommandPart[] LF(string key, string value = null, char quoteChar = default) => new[] { TCP(key, false, true, value, quoteChar) };
        TestCommandPart[] SF(char key, string value = default, char quoteChar = default) => new[] { TCP(key.ToString(), true, false, value, quoteChar) };
        IEnumerable<TestCommandPart> SF(string key)
        {
            foreach(var keyChar in key)
            {
                yield return TCP(keyChar.ToString(), true, false);
            }
        }
        TestCommandPart[] V(string value, char quoteChar = default) => new[] { TCP(null, false, false, value, quoteChar) };
        
        [Fact]
        public void ArgumentTests()
        {
            var commandsAndResults = new (string Command, IEnumerable<CommandPart> Result)[]
            {
                //Long Form
                TestSetup(LF("test")),
                TestSetup(LF("test", "value")),
                TestSetup(LF("test", "123")),
                TestSetup(LF("test", "value with spaces", '"')),
                TestSetup(LF("test", "value with spaces", '\'')),

                TestSetup(LF("test"), LF("test")),
                TestSetup(LF("test", "value"), LF("test", "value")),
                TestSetup(LF("test", "123"), LF("test", "123")),
                TestSetup(LF("test", "value with spaces", '"'), LF("test", "value with spaces", '"')),
                TestSetup(LF("test", "value with spaces", '\''), LF("test", "value with spaces", '\'')),

                TestSetup(LF("test"), LF("test", "value with spaces", '"')),
                TestSetup(LF("test", "value with spaces", '"'), LF("test")),

                //Short Form
                TestSetup(SF("t")),
                TestSetup(SF('t', "value")),
                TestSetup(SF('t', "123")),
                TestSetup(SF('t', "value with spaces", '"')),

                TestSetup(SF('t'), SF("t")),
                TestSetup(SF('t', "value"), SF('t', "value")),
                TestSetup(SF('t', "123"), SF('t', "123")),
                TestSetup(SF('t', "value with spaces", '"'), SF('t', "value with spaces", '"')),

                TestSetup(SF('t'), SF('t', "value with spaces", '"')),
                TestSetup(SF('t', "value with spaces", '"'), SF("t")),

                TestSetup(SF("tt")),
                TestSetup(SF("tt"), SF("tt")),

                //Value
                TestSetup(V("test")),
                TestSetup(V("test", '\"')),
                TestSetup(V("test", '\'')),
                TestSetup(V("123")),

                TestSetup(V("test"), V("test")),
                TestSetup(V("test", '\"'), V("test", '\"')),
                TestSetup(V("test", '\''), V("test", '\'')),
                TestSetup(V("123"), V("123")),

                //Command tests
                TestSetup(V("command"), V("argvalue"), SF("f"), LF("long-form", "with value", '\'')),
                TestSetup(V("command"), V("argvalue"), LF("long-form", "with value", '\''), SF("f")),
                TestSetup(V("command"), V("argvalue"), V("argvalue 2 with spcaes", '"'), LF("long-form", "with value", '\''), SF("f")),

                TestSetup(V("command"), V("argvalue"), LF("key-no-val"), LF("long-form", "with value", '\''), SF("abc"), SF('d', "value"), SF("ef"), LF("key-no-val"))
            };

            foreach (var commandAndResult in commandsAndResults)
            {
                var sut = CreateSut(commandAndResult.Command);

                var result = sut.Parse();

                result.Should().BeEquivalentTo(commandAndResult.Result);
            }
        }
    }
}

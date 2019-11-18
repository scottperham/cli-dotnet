using FluentAssertions;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace cli_dotnet.test.unit
{
    public class ParserImplTests
    {
        public CommandPart CP(bool arg, bool sf, int ks = default, int kl = default, int vs = default, int vl = default)
        {
            return new CommandPart
            {
                IsArgument = arg,
                IsShortForm = sf,
                Key = new StringReference
                {
                    Start = ks,
                    Length = kl
                },
                Value = new StringReference
                {
                    Start = vs,
                    Length = vl
                }
            };
        }

        public ICommandParserImplementation CreateSutImpl(string command = null, ICommandParserImplementation impl = null)
        {
            command ??= "";
            impl ??= Substitute.For<ICommandParserImplementation>();

            return new CommandParser(command, impl);
        }

        public ICommandParser CreateSut(string command = null, ICommandParserImplementation impl = null)
        {
            command ??= "";
            impl ??= Substitute.For<ICommandParserImplementation>();

            return new CommandParser(command, impl);
        }

        [Fact]
        public void Ctor_SetsCorrectPosition()
        {
            var impl = Substitute.For<ICommandParserImplementation>();

            CreateSutImpl(impl: impl);

            impl.Received().Position = -1;
        }

        [Fact]
        public void Ctor_WhenCommandIsNull_ThrowsArgumentNullException()
        {
            Action act = () => new CommandParser(null);

            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Ctor_SetsCommand()
        {
            var impl = Substitute.For<ICommandParserImplementation>();
            var command = "test command";

            CreateSutImpl(command, impl);

            impl.Received().Command = command;
        }

        [Fact]
        public void Peek_WhenPositionInCommand_ReturnsCorrectCharAndDoesntMovePosition()
        {
            var impl = Substitute.For<ICommandParserImplementation>();

            var sut = CreateSutImpl(impl: impl);

            impl.ClearReceivedCalls();
            impl.Command.Returns("test command");
            impl.Position.Returns(1);

            var ch = sut.Peek();

            impl.DidNotReceiveWithAnyArgs().Position = Arg.Any<int>();
            ch.Should().Be('s');
        }

        [Fact]
        public void Peek_WhenPositionAtEof_ReturnsCorrectCharAndDoesntMovePosition()
        {
            var impl = Substitute.For<ICommandParserImplementation>();

            var sut = CreateSutImpl(impl: impl);

            impl.ClearReceivedCalls();
            impl.Command.Returns("test command");
            impl.Position.Returns(11);

            var ch = sut.Peek();

            impl.DidNotReceiveWithAnyArgs().Position = Arg.Any<int>();
            ch.Should().Be('\0');
        }

        [Fact]
        public void Peek_WhenPositionBeyondEof_ReturnsEofAndDoesntMovePosition()
        {
            var impl = Substitute.For<ICommandParserImplementation>();

            var sut = CreateSutImpl(impl: impl);

            impl.ClearReceivedCalls();
            impl.Command.Returns("test command");
            impl.Position.Returns(100);

            var ch = sut.Peek();

            impl.DidNotReceiveWithAnyArgs().Position = Arg.Any<int>();
            ch.Should().Be('\0');
        }

        [Fact]
        public void Consume_WhenPositionInCommand_MovesPosition()
        {
            var impl = Substitute.For<ICommandParserImplementation>();

            var sut = CreateSutImpl(impl: impl);

            impl.ClearReceivedCalls();
            impl.Command.Returns("test command");
            impl.Position.Returns(1);

            sut.Consume();

            impl.Received().Position = 2;
        }

        [Fact]
        public void Consume_WhenPositionAtEof_MovesPositionToEof()
        {
            var impl = Substitute.For<ICommandParserImplementation>();

            var sut = CreateSutImpl(impl: impl);

            impl.ClearReceivedCalls();
            impl.Command.Returns("test command");
            impl.Position.Returns(12);

            sut.Consume();

            impl.Received().Position = 12;
        }

        [Fact]
        public void Consume_WhenPositionBeyondEof_MovesPositionToEof()
        {
            var impl = Substitute.For<ICommandParserImplementation>();

            var sut = CreateSutImpl(impl: impl);

            impl.ClearReceivedCalls();
            impl.Command.Returns("test command");
            impl.Position.Returns(100);

            sut.Consume();

            impl.Received().Position = 12;
        }

        [Fact]
        public void GetString_WhenStringRefAtStart_ReturnsCorrectString()
        {
            var impl = Substitute.For<ICommandParserImplementation>();

            var sut = CreateSut(impl: impl);

            impl.ClearReceivedCalls();
            impl.Command.Returns("test command");

            var result = sut.GetString(new StringReference
            {
                Start = 0,
                Length = 4
            });

            result.Should().Be("test");
        }

        [Fact]
        public void GetString_WhenStringRefAtEnd_ReturnsCorrectString()
        {
            var impl = Substitute.For<ICommandParserImplementation>();

            var sut = CreateSut(impl: impl);

            impl.ClearReceivedCalls();
            impl.Command.Returns("test command");

            var result = sut.GetString(new StringReference
            {
                Start = 5,
                Length = 7
            });

            result.Should().Be("command");
        }

        [Fact]
        public void GetString_WhenStringRefInMiddle_ReturnsCorrectString()
        {
            var impl = Substitute.For<ICommandParserImplementation>();

            var sut = CreateSut(impl: impl);

            impl.ClearReceivedCalls();
            impl.Command.Returns("test command");

            var result = sut.GetString(new StringReference
            {
                Start = 2,
                Length = 7
            });

            result.Should().Be("st comm");
        }

        [Fact]
        public void GetString_WhenStringRefBeforeStart_ThrowsArgumentOutOfRangeException()
        {
            var impl = Substitute.For<ICommandParserImplementation>();

            var sut = CreateSut(impl: impl);

            impl.ClearReceivedCalls();
            impl.Command.Returns("test command");

            Action action = () => sut.GetString(new StringReference
            {
                Start = -1,
                Length = 7
            });

            action.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Fact]
        public void GetString_WhenStringRefBeyondEnd_ThrowsArgumentOutOfRangeException()
        {
            var impl = Substitute.For<ICommandParserImplementation>();

            var sut = CreateSut(impl: impl);

            impl.ClearReceivedCalls();
            impl.Command.Returns("test command");

            Action action = () => sut.GetString(new StringReference
            {
                Start = 0,
                Length = 100
            });

            action.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Fact]
        public void ConsumeIf_WhenNextCharCorrect_MovesPosition()
        {
            var impl = Substitute.For<ICommandParserImplementation>();

            var ch = 't';

            var sut = CreateSutImpl(impl: impl);

            impl.ClearReceivedCalls();
            impl.Peek().Returns(ch);
            impl.Position.Returns(1);

            sut.ConsumeIf(ch);

            impl.Received().Position = 2;
        }

        [Fact]
        public void ConsumeIf_WhenNextCharWrong_DoesntMovesPosition()
        {
            var impl = Substitute.For<ICommandParserImplementation>();

            var ch = 't';

            var sut = CreateSutImpl(impl: impl);

            impl.ClearReceivedCalls();
            impl.Peek().Returns(ch);
            impl.Position.Returns(1);

            sut.ConsumeIf('r');

            impl.DidNotReceiveWithAnyArgs().Position = Arg.Any<int>();
        }

        [Fact]
        public void ConsumeWhitespace_WhenNotWhitespace_DoesntMovesPosition()
        {
            var impl = Substitute.For<ICommandParserImplementation>();

            var sut = CreateSutImpl(impl: impl);

            impl.ClearReceivedCalls();
            impl.Peek().Returns('t');

            sut.ConsumeWhitespace();

            impl.DidNotReceive().Consume();
            impl.DidNotReceiveWithAnyArgs().Position = Arg.Any<int>();
        }

        [Fact]
        public void ConsumeWhitespace_WhenWhitespace_MovesPosition()
        {
            var impl = Substitute.For<ICommandParserImplementation>();

            var sut = CreateSutImpl(impl: impl);

            impl.ClearReceivedCalls();
            impl.Peek().Returns(' ', '\t', '\n', '\r', 't');

            sut.ConsumeWhitespace();

            impl.Received(4).Consume();
        }

        [Fact]
        public void Parse_WhenNoCommandParts_YieldsNothing()
        {
            var impl = Substitute.For<ICommandParserImplementation>();

            var sut = CreateSut(impl: impl);

            impl.ClearReceivedCalls();

            impl.TryGetCommandPart(out _).ReturnsForAnyArgs(false);

            var result = sut.Parse();

            result.Should().BeEmpty();
        }

        [Fact]
        public void Parse_WhenCommandPartIsLongForm_YieldsCommandPart()
        {
            var impl = Substitute.For<ICommandParserImplementation>();

            var sut = CreateSut(impl: impl);

            impl.ClearReceivedCalls();

            var cp = CP(true, false);

            impl.TryGetCommandPart(out _).ReturnsForAnyArgs(x =>
            {
                x[0] = cp;

                return true;
            }, x => false);

            var result = sut.Parse();

            result.Should().BeEquivalentTo(cp);
        }

        [Fact]
        public void Parse_WhenCommandPartIsValue_YieldsCommandPart()
        {
            var impl = Substitute.For<ICommandParserImplementation>();

            var sut = CreateSut(impl: impl);

            impl.ClearReceivedCalls();

            var cp = CP(false, false);

            impl.TryGetCommandPart(out _).ReturnsForAnyArgs(x =>
            {
                x[0] = cp;

                return true;
            }, x => false);

            var result = sut.Parse();

            result.Should().BeEquivalentTo(cp);
        }

        [Fact]
        public void Parse_WhenCommandPartContainsSingleShortForm_YieldsCorrectCommandPart()
        {
            var impl = Substitute.For<ICommandParserImplementation>();

            var sut = CreateSut(impl: impl);

            impl.ClearReceivedCalls();

            var cp = CP(true, true, 0, 1, 10, 100);

            impl.TryGetCommandPart(out _).ReturnsForAnyArgs(x =>
            {
                x[0] = cp;

                return true;
            }, x => false);

            var result = sut.Parse();

            result.Should().BeEquivalentTo(cp);
        }

        [Fact]
        public void Parse_WhenCommandPartContainsMultipleShortForm_YieldsCorrectCommandPart()
        {
            var impl = Substitute.For<ICommandParserImplementation>();

            var sut = CreateSut(impl: impl);

            impl.ClearReceivedCalls();

            impl.TryGetCommandPart(out _).ReturnsForAnyArgs(x =>
            {
                x[0] = CP(true, true, 0, 3);

                return true;
            }, x => false);

            var result = sut.Parse();

            result.Should().BeEquivalentTo(
                CP(true, true, 0, 1),
                CP(true, true, 1, 1),
                CP(true, true, 2, 1));
        }

        [Fact]
        public void Parse_WhenCommandPartContainsMultipleShortFormAndValue_ThrowsBadCommandException()
        {
            var impl = Substitute.For<ICommandParserImplementation>();

            var sut = CreateSut(impl: impl);

            impl.ClearReceivedCalls();

            impl.TryGetCommandPart(out _).ReturnsForAnyArgs(x =>
            {
                x[0] = CP(true, true, 0, 3, 10, 100);

                return true;
            }, x => false);

            sut.Parse();

            var result = sut.Parse();

            Action act = () => result.ToArray();

            act.Should().Throw<BadCommandException>();
        }

        [Fact]
        public void Parse_WhenMultipleCommandParts_AllPartsYielded()
        {
            var impl = Substitute.For<ICommandParserImplementation>();

            var sut = CreateSut(impl: impl);

            impl.ClearReceivedCalls();

            var cp1 = CP(false, false, 1, 2);
            var cp2 = CP(true, false, 3, 4, 5, 6);

            impl.TryGetCommandPart(out _).ReturnsForAnyArgs(x =>
            {
                x[0] = cp1;

                return true;
            }, x =>
            {
                x[0] = cp2;

                return true;
            }, x => false);

            var result = sut.Parse();

            result.Should().BeEquivalentTo(cp1, cp2);
        }

        [Fact]
        public void ScanTo_WhenCharInCommand_ReturnsCorrectStringRefAndPosition()
        {
            var impl = Substitute.For<ICommandParserImplementation>();

            var sut = CreateSutImpl(impl: impl);

            impl.ClearReceivedCalls();

            var command = "test command";

            var position = -1;

            impl.Peek().Returns(x => position >= command.Length - 1 ? '\0' : command[position + 1]);
            impl.Command.Returns(command);
            impl.Position.Returns(x => position);
            impl.When(x => x.Consume()).Do(x => ++position);

            var result = sut.ScanTo(' ');
            result.Should().Be(new StringReference(0, 4));
            position.Should().Be(3);
        }

        [Fact]
        public void ScanTo_WhenCharInCommandAndEscaped_ReturnsCorrectStringRefAndPosition()
        {
            var impl = Substitute.For<ICommandParserImplementation>();

            var sut = CreateSutImpl(impl: impl);

            impl.ClearReceivedCalls();

            var command = "test com mand";

            var position = -1;

            impl.Peek().Returns(x => position >= command.Length - 1 ? '\0' : command[position + 1]);
            impl.Command.Returns(command);
            impl.Position.Returns(x => position);
            impl.When(x => x.Consume()).Do(x => ++position);

            var result = sut.ScanTo(' ', 't');
            result.Should().Be(new StringReference(0, 8));
            position.Should().Be(7);
        }

        [Fact]
        public void ScanTo_WhenCharNotInCommand_ReturnsCorrectStringRefAndPosition()
        {
            var impl = Substitute.For<ICommandParserImplementation>();

            var sut = CreateSutImpl(impl: impl);

            impl.ClearReceivedCalls();

            var command = "test command";

            var position = -1;

            impl.Peek().Returns(x => position >= command.Length - 1 ? '\0' : command[position + 1]);
            impl.Command.Returns(command);
            impl.Position.Returns(x => position);
            impl.When(x => x.Consume()).Do(x => ++position);

            var result = sut.ScanTo('x');
            result.Should().Be(new StringReference(0, 12));
            position.Should().Be(11);
        }

        [Fact]
        public void ScanTo_WhenCharEscapedInCommand_ReturnsCorrectStringRefAndPosition()
        {
            var impl = Substitute.For<ICommandParserImplementation>();

            var sut = CreateSutImpl(impl: impl);

            impl.ClearReceivedCalls();

            var command = "test command";

            var position = -1;

            impl.Peek().Returns(x => position >= command.Length - 1 ? '\0' : command[position + 1]);
            impl.Command.Returns(command);
            impl.Position.Returns(x => position);
            impl.When(x => x.Consume()).Do(x => ++position);

            var result = sut.ScanTo(' ', 't');
            result.Should().Be(new StringReference(0, 12));
            position.Should().Be(11);
        }

        [Fact]
        public void ScanToAny_SingleChar_ReturnsCorrectStringRefAndPosition()
        {
            var impl = Substitute.For<ICommandParserImplementation>();

            var sut = CreateSutImpl(impl: impl);

            impl.ClearReceivedCalls();

            var command = "test command";

            var position = -1;

            impl.Peek().Returns(x => position >= command.Length - 1 ? '\0' : command[position + 1]);
            impl.Position.Returns(x => position);
            impl.When(x => x.Consume()).Do(x => ++position);

            var result = sut.ScanToAny(' ');
            result.Should().Be(new StringReference(0, 4));
            position.Should().Be(3);
        }

        [Fact]
        public void ScanToAny_MultiChar_ReturnsCorrectStringRefAndPosition()
        {
            var impl = Substitute.For<ICommandParserImplementation>();

            var sut = CreateSutImpl(impl: impl);

            impl.ClearReceivedCalls();

            var command = "test command";

            var position = -1;

            impl.Peek().Returns(x => position >= command.Length - 1 ? '\0' : command[position + 1]);
            impl.Position.Returns(x => position);
            impl.When(x => x.Consume()).Do(x => ++position);

            var result = sut.ScanToAny('o', ' ');
            result.Should().Be(new StringReference(0, 4));
            position.Should().Be(3);
        }

        [Fact]
        public void GetValue_ReturnsCorrectCommandPart()
        {
            var impl = Substitute.For<ICommandParserImplementation>();

            var sut = CreateSutImpl(impl: impl);

            impl.ClearReceivedCalls();

            var start = 10;
            var length = 100;

            impl.GetValueReference(true).Returns(new StringReference(start, length));

            var result = sut.GetValue();

            result.Should().Be(CP(false, false, start, length));
        }

        [Fact]
        public void GetShortFormArgument_WhenSingleKeyNoValue_ReturnsCorrectCommandPart()
        {
            var impl = Substitute.For<ICommandParserImplementation>();

            var sut = CreateSutImpl(impl: impl);

            impl.ClearReceivedCalls();

            var key = new StringReference(10, 100);

            impl.GetValueReference(false).Returns(key);
            impl.Peek().Returns('-');

            var result = sut.GetShortFormArgument();

            result.Should().Be(CP(true, true, 10, 100));
        }

        [Fact]
        public void GetShortFormArgument_WhenSingleKeyWithValue_ReturnsCorrectCommandPart()
        {
            var impl = Substitute.For<ICommandParserImplementation>();

            var sut = CreateSutImpl(impl: impl);

            impl.ClearReceivedCalls();

            var key = new StringReference(10, 1);
            var value = new StringReference(20, 200);

            impl.Peek().Returns('t');
            impl.GetValueReference(false).Returns(key);
            impl.GetValueReference(true).Returns(value);

            var result = sut.GetShortFormArgument();

            result.Should().Be(CP(true, true, 10, 1, 20, 200));
        }

        [Fact]
        public void GetLongFormArgument_WhenNoValue_ReturnsCorrectCommandPart()
        {
            var impl = Substitute.For<ICommandParserImplementation>();

            var sut = CreateSutImpl(impl: impl);

            impl.ClearReceivedCalls();

            var key = new StringReference(10, 1);

            impl.ScanToAny('=', ' ').Returns(key);

            var result = sut.GetLongFormArgument();

            result.Should().Be(CP(true, false, 10, 1));
        }

        [Fact]
        public void GetLongFormArgument_WhenValue_ReturnsCorrectCommandPart()
        {
            var impl = Substitute.For<ICommandParserImplementation>();

            var sut = CreateSutImpl(impl: impl);

            impl.ClearReceivedCalls();

            var key = new StringReference(10, 100);
            var value = new StringReference(20, 200);

            impl.ScanToAny('=', ' ').Returns(key);
            impl.Peek().Returns('=');
            impl.GetValueReference(true).Returns(value);

            var result = sut.GetLongFormArgument();

            impl.Received(1).Consume();
            result.Should().Be(CP(true, false, 10, 100, 20, 200));
        }

        [Fact]
        public void GetValueReference_WhenNoQuotes_ReturnsCorrectStringReference()
        {
            var impl = Substitute.For<ICommandParserImplementation>();

            var sut = CreateSutImpl(impl: impl);

            impl.ClearReceivedCalls();

            var value = new StringReference(10, 100);

            impl.ScanTo(' ').Returns(value);

            var result = sut.GetValueReference(false);

            result.Should().Be(value);
        }

        [Fact]
        public void GetValueReference_WhenAllowedQuotesButNone_ReturnsCorrectStringReference()
        {
            var impl = Substitute.For<ICommandParserImplementation>();

            var sut = CreateSutImpl(impl: impl);

            impl.ClearReceivedCalls();

            var value = new StringReference(10, 100);

            impl.Peek().Returns('t');
            impl.ScanTo(' ').Returns(value);

            var result = sut.GetValueReference(true);

            result.Should().Be(value);
        }

        [Fact]
        public void GetValueReference_WithQuotes_ReturnsCorrectStringReference()
        {
            var impl = Substitute.For<ICommandParserImplementation>();

            var sut = CreateSutImpl(impl: impl);

            impl.ClearReceivedCalls();

            var value = new StringReference(20, 200);

            impl.ScanTo('\"', '\\').Returns(value);
            impl.Peek().Returns('\"');

            var result = sut.GetValueReference(true);

            impl.Received(2).Consume();

            result.Should().Be(value);
        }

        [Fact]
        public void TryGetCommandPart_WhenEmptyCommand_ReturnsFalse()
        {
            var impl = Substitute.For<ICommandParserImplementation>();

            var sut = CreateSutImpl(impl: impl);

            impl.ClearReceivedCalls();

            impl.Peek().Returns('\0');

            var ret = sut.TryGetCommandPart(out _);

            impl.Received(1).ConsumeWhitespace();

            ret.Should().BeFalse();
        }

        [Fact]
        public void TryGetCommandPart_WhenShortForm_ReturnsCorrectCommandPart()
        {
            var impl = Substitute.For<ICommandParserImplementation>();

            var sut = CreateSutImpl(impl: impl);

            impl.ClearReceivedCalls();

            var cp = CP(true, true, 10, 1);

            impl.Peek().Returns('-');
            impl.ConsumeIf('-').Returns(true, false);
            impl.GetShortFormArgument().Returns(cp);

            var ret = sut.TryGetCommandPart(out var result);

            ret.Should().BeTrue();
            result.Should().Be(cp);
        }

        [Fact]
        public void TryGetCommandPart_WhenLongForm_ReturnsCorrectCommandPart()
        {
            var impl = Substitute.For<ICommandParserImplementation>();

            var sut = CreateSutImpl(impl: impl);

            impl.ClearReceivedCalls();

            var cp = CP(true, true, 10, 1);

            impl.Peek().Returns('-');
            impl.ConsumeIf('-').Returns(true, true);
            impl.GetLongFormArgument().Returns(cp);

            var ret = sut.TryGetCommandPart(out var result);

            ret.Should().BeTrue();
            result.Should().Be(cp);
        }

        [Fact]
        public void TryGetCommandPart_WhenValue_ReturnsCorrectCommandPart()
        {
            var impl = Substitute.For<ICommandParserImplementation>();

            var sut = CreateSutImpl(impl: impl);

            impl.ClearReceivedCalls();

            var cp = CP(true, true, 10, 1);

            impl.Peek().Returns('-');
            impl.ConsumeIf('-').Returns(false);
            impl.GetValue().Returns(cp);

            var ret = sut.TryGetCommandPart(out var result);

            ret.Should().BeTrue();
            result.Should().Be(cp);
        }
    }
}

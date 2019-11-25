using FluentAssertions;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace cli_dotnet.test.unit
{
    public class CommandExecutorTests
    {
        public ICommandExecutorImpl CreateSutImpl(ICommandParser parser = null, ICommandExecutorOptions executorOptions = null, IAttributeDecorator attributeDecorator = null, IValueConverter valueConverter = null, ICommandHelper commandHelper = null, ICommandExecutorImpl impl = null)
        {
            return new CommandExecutor(parser ?? Substitute.For<ICommandParser>(),
                                        executorOptions ?? Substitute.For<ICommandExecutorOptions>(),
                                        attributeDecorator ?? Substitute.For<IAttributeDecorator>(),
                                        valueConverter ?? Substitute.For<IValueConverter>(),
                                        commandHelper ?? Substitute.For<ICommandHelper>(),
                                        impl);
        }

        public CommandExecutor CreateSut(ICommandParser parser = null, ICommandExecutorOptions executorOptions = null, IAttributeDecorator attributeDecorator = null, IValueConverter valueConverter = null, ICommandHelper commandHelper = null, ICommandExecutorImpl impl = null)
        {
            return new CommandExecutor(parser ?? Substitute.For<ICommandParser>(),
                                        executorOptions ?? Substitute.For<ICommandExecutorOptions>(),
                                        attributeDecorator ?? Substitute.For<IAttributeDecorator>(),
                                        valueConverter ?? Substitute.For<IValueConverter>(),
                                        commandHelper ?? Substitute.For<ICommandHelper>(),
                                        impl);
        }

        [Fact]
        async public Task ExecuteAsync_WrapsRootCommandAndCallsExecuteInternal()
        {
            var commandParts = new List<CommandPart>();

            var parser = Substitute.For<ICommandParser>();
            parser.Parse().Returns(commandParts);

            var attributeDecorator = Substitute.For<IAttributeDecorator>();

            var impl = Substitute.For<ICommandExecutorImpl>();

            var sut = CreateSut(parser, attributeDecorator: attributeDecorator, impl: impl);

            var rootCommand = new object();

            await sut.ExecuteAsync(rootCommand);

            attributeDecorator.Received(1).Decorate(Arg.Is<VerbAttribute>(x => x.Instance == rootCommand && x.IsRoot));

            await impl.ReceivedWithAnyArgs().ExecuteInternalAsync(Arg.Is<VerbAttribute>(x => x.Instance == rootCommand && x.IsRoot), Arg.Any<IEnumerator<CommandPart>>());
        }

        [Fact]
        async public Task ExecuteAsync_WhenExecuteInternalThrowsCommandError_ShowsHelp()
        {
            var parser = Substitute.For<ICommandParser>();
            parser.Parse().Returns(new CommandPart[0]);

            var options = Substitute.For<ICommandExecutorOptions>();

            var helper = Substitute.For<ICommandHelper>();

            var impl = Substitute.For<ICommandExecutorImpl>();

            var badCommand = new CommandAttribute();
            impl.WhenForAnyArgs(x => x.ExecuteInternalAsync(default, default)).Throw(new BadCommandException(badCommand, default));

            var sut = CreateSut(parser, executorOptions: options, commandHelper: helper, impl: impl);

            await sut.ExecuteAsync(new object());

            helper.Received(1).WriteCommandHelp(badCommand, options);
        }

        [Fact]
        async public Task ExecuteAsync_WhenExecuteInternalThrowsVerbError_ShowsHelp()
        {
            var parser = Substitute.For<ICommandParser>();
            parser.Parse().Returns(new CommandPart[0]);

            var options = Substitute.For<ICommandExecutorOptions>();

            var helper = Substitute.For<ICommandHelper>();

            var impl = Substitute.For<ICommandExecutorImpl>();

            var badVerb = new VerbAttribute();
            impl.WhenForAnyArgs(x => x.ExecuteInternalAsync(default, default)).Throw(new BadCommandException(badVerb, default));

            var sut = CreateSut(parser, executorOptions: options, commandHelper: helper, impl: impl);

            await sut.ExecuteAsync(new object());

            helper.Received(1).WriteVerbHelp(badVerb, options);
        }

        [Fact]
        public void ExecuteAsync_WhenExecuteInternalThrowsGenericError_Throws()
        {
            var parser = Substitute.For<ICommandParser>();
            parser.Parse().Returns(new CommandPart[0]);

            var options = Substitute.For<ICommandExecutorOptions>();

            var helper = Substitute.For<ICommandHelper>();

            var impl = Substitute.For<ICommandExecutorImpl>();

            var exception = new Exception();
            impl.WhenForAnyArgs(x => x.ExecuteInternalAsync(default, default)).Throw(exception);

            var sut = CreateSut(parser, executorOptions: options, commandHelper: helper, impl: impl);

            Func<Task> act = () => sut.ExecuteAsync(new object());

            act.Should().Throw<Exception>();
        }

        [Fact]
        public void ExecuteInternalAsync_WhenEmptyCommands_Throws()
        {
            var commandParts = Substitute.For<IEnumerator<CommandPart>>();

            var verb = new VerbAttribute();

            var sut = CreateSutImpl();

            Func<Task> act = () => sut.ExecuteInternalAsync(verb, commandParts);

            act.Should().Throw<BadCommandException>();
        }

        [Fact]
        public void ExecuteInternalAsync_WhenIsArgument_Throws()
        {
            var commandParts = new List<CommandPart>(new[]
            {
                new CommandPart{ IsArgument = true }
            });

            var verb = new VerbAttribute();

            var sut = CreateSutImpl();

            Func<Task> act = () => sut.ExecuteInternalAsync(verb, commandParts.GetEnumerator());

            act.Should().Throw<BadCommandException>();
        }

        [Fact]
        async public Task ExecuteInternalAsync_WhenCanHelp_DoesntThrow()
        {
            var commandPart = new CommandPart { IsArgument = true, Key = new StringReference { Start = 10, Length = 100 } };
            var key = "test";

            var parser = Substitute.For<ICommandParser>();
            parser.GetString(commandPart.Key).Returns(key);

            var options = Substitute.For<ICommandExecutorOptions>();

            var verb = new VerbAttribute();

            var commandHelper = Substitute.For<ICommandHelper>();
            commandHelper.TryShowHelp(commandPart, verb, key, options).Returns(true);

            var commandParts = new List<CommandPart>(new[] { commandPart });

            var sut = CreateSutImpl(parser, options, commandHelper: commandHelper);

            await sut.ExecuteInternalAsync(verb, commandParts.GetEnumerator());
        }

        [Fact]
        public void ExecuteInternalAsync_WhenCantHelp_Throws()
        {
            var commandPart = new CommandPart { IsArgument = true, Key = new StringReference { Start = 10, Length = 100 } };
            var key = "test";

            var parser = Substitute.For<ICommandParser>();
            parser.GetString(commandPart.Key).Returns(key);

            var options = Substitute.For<ICommandExecutorOptions>();

            var verb = new VerbAttribute();

            var commandHelper = Substitute.For<ICommandHelper>();
            commandHelper.TryShowHelp(commandPart, verb, key, options).Returns(false);

            var commandParts = new List<CommandPart>(new[] { commandPart });

            var sut = CreateSutImpl(parser, options, commandHelper: commandHelper);

            Func<Task> act = () => sut.ExecuteInternalAsync(verb, commandParts.GetEnumerator());

            act.Should().Throw<BadCommandException>();
        }

        [Fact]
        async public Task ExecuteInternalAsync_WhenIsVerb_Recurse()
        {
            var commandPart = new CommandPart { IsArgument = false, Key = new StringReference { Start = 10, Length = 100 } };
            var key = "test";

            var parser = Substitute.For<ICommandParser>();
            parser.GetString(commandPart.Key).Returns(key);

            var impl = Substitute.For<ICommandExecutorImpl>();

            var innerVerb = new VerbAttribute();

            var verb = new VerbAttribute();
            verb.Verbs.Add(key, innerVerb);

            var commandParts = new List<CommandPart>(new[] { commandPart });

            var sut = CreateSutImpl(parser, impl: impl);

            await sut.ExecuteInternalAsync(verb, commandParts.GetEnumerator());

            await impl.Received(1).ExecuteInternalAsync(innerVerb, Arg.Any<IEnumerator<CommandPart>>());
        }

        [Fact]
        async public Task ExecuteInternalAsync_WhenIsCommand_CallExecuteCommand()
        {
            var commandPart = new CommandPart { IsArgument = false, Key = new StringReference { Start = 10, Length = 100 } };
            var key = "test";

            var parser = Substitute.For<ICommandParser>();
            parser.GetString(commandPart.Key).Returns(key);

            var impl = Substitute.For<ICommandExecutorImpl>();

            var command = new CommandAttribute();

            var verb = new VerbAttribute();
            verb.Commands.Add(key, command);

            var commandParts = new List<CommandPart>(new[] { commandPart });

            var sut = CreateSutImpl(parser, impl: impl);

            await sut.ExecuteInternalAsync(verb, commandParts.GetEnumerator());

            await impl.Received(1).ExecuteCommandAsync(command, Arg.Any<IEnumerator<CommandPart>>());
        }

        [Fact]
        public void ExecuteInternalAsync_WhenNotFound_Throws()
        {
            var commandPart = new CommandPart { IsArgument = false };

            var command = new CommandAttribute();

            var verb = new VerbAttribute();

            var commandParts = new List<CommandPart>(new[] { commandPart });

            var sut = CreateSutImpl();

            Func<Task> act = () => sut.ExecuteInternalAsync(verb, commandParts.GetEnumerator());

            act.Should().Throw<BadCommandException>();
        }

        [Fact]
        public void TryAddToLastParameter_WhenLastParameterNull_ReturnsFalse()
        {
            var sut = CreateSutImpl();

            var result = sut.TryAddToLastParameter(null, new TestParameterInfo(), "hello", new SortedList<int, object>());

            result.Should().BeFalse();
        }

        [Fact]
        public void TryAddToLastParameter_WhenLastParameterNotArrayAndThisParameterNull_ReturnsFalse()
        {
            var list = new SortedList<int, object>();
            var parameter = new TestParameterInfo("test", typeof(string));

            var sut = CreateSutImpl();

            var result = sut.TryAddToLastParameter(parameter, null, "hello", list);

            result.Should().BeFalse();
            list.Should().BeEmpty();
        }

        [Fact]
        public void TryAddToLastParameter_WhenLastParameterNotArrayAndThisParameterNotNull_ReturnsFalse_AndSetsValue()
        {
            var list = new SortedList<int, object>();
            var parameter = new TestParameterInfo("test", typeof(string));
            var thisParameter = new TestParameterInfo("thisValue", typeof(int), 10);
            var value = "42";
            var actualValue = 42;

            var valueConverter = Substitute.For<IValueConverter>();
            valueConverter.GetValue(value, typeof(int)).Returns(actualValue);

            var sut = CreateSutImpl(valueConverter: valueConverter);

            var result = sut.TryAddToLastParameter(parameter, thisParameter, value, list);

            result.Should().BeFalse();
            list.Count.Should().Be(1);
            list[10].Should().Be(actualValue);
        }

        [Fact]
        public void TryAddToLastParameter_WhenLastParameterListEmpty_ReturnsTrue_AndSetsValue()
        {
            var value = "hello";
            var position = 5;
            var parameters = new SortedList<int, object>();

            var parameter = new TestParameterInfo("test", typeof(string[]), position);

            var valueConverter = Substitute.For<IValueConverter>();
            valueConverter.GetValue(value, typeof(string)).Returns(value);

            var sut = CreateSutImpl(valueConverter: valueConverter);

            var result = sut.TryAddToLastParameter(parameter, null, value, parameters);

            result.Should().BeTrue();
            parameters.Count.Should().Be(1);
            parameters[position].Should().BeEquivalentTo(new string[] { value });
        }

        [Fact]
        public void TryAddToLastParameter_WhenLastParameterEmptyArray_ReturnsTrue_AndSetsValue()
        {
            var value = "hello";
            var position = 5;
            var parameters = new SortedList<int, object>();
            parameters.Add(position, new string[0]);

            var parameter = new TestParameterInfo("test", typeof(string[]), position);

            var valueConverter = Substitute.For<IValueConverter>();
            valueConverter.GetValue(value, typeof(string)).Returns(value);

            var sut = CreateSutImpl(valueConverter: valueConverter);

            var result = sut.TryAddToLastParameter(parameter, null, value, parameters);

            result.Should().BeTrue();
            parameters.Count.Should().Be(1);
            parameters[position].Should().BeEquivalentTo(new string[] { value });
        }

        [Fact]
        public void TryAddToLastParameter_WhenLastParameterNotEmptyArray_ReturnsTrue_AndSetsValue()
        {
            var value = "hello";
            var existingValue = new[] { "test1", "test2" };
            var position = 5;
            var parameters = new SortedList<int, object>();
            parameters.Add(position, existingValue);

            var parameter = new TestParameterInfo("test", typeof(string[]), position);

            var valueConverter = Substitute.For<IValueConverter>();
            valueConverter.GetValue(value, typeof(string)).Returns(value);

            var sut = CreateSutImpl(valueConverter: valueConverter);

            var result = sut.TryAddToLastParameter(parameter, null, value, parameters);

            result.Should().BeTrue();
            parameters.Count.Should().Be(1);
            parameters[position].Should().BeEquivalentTo(new string[] { existingValue[0], existingValue[1], value });
        }
    }
}

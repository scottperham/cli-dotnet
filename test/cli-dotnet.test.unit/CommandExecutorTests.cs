using FluentAssertions;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

        [Command]
        static void Method() { }

        [Fact]
        async public Task ExecuteAsync_MethodInfo_WrapsCommandAndCallsExecuteInternal()
        {
            var commandParts = new List<CommandPart>();

            var parser = Substitute.For<ICommandParser>();
            parser.Parse().Returns(commandParts);

            var attributeDecorator = Substitute.For<IAttributeDecorator>();

            var impl = Substitute.For<ICommandExecutorImpl>();

            var sut = CreateSut(parser, attributeDecorator: attributeDecorator, impl: impl);
            
            var commandMethod = ((Action)Method).GetMethodInfo();

            await sut.ExecuteAsync(commandMethod);

            attributeDecorator.Received(1).Decorate(Arg.Is<CommandAttribute>(x => x.Method == commandMethod));

            await impl.ReceivedWithAnyArgs().ExecuteCommandAsync(Arg.Is<CommandAttribute>(x => x.Method == commandMethod), Arg.Any<IEnumerator<CommandPart>>());
        }
        [Fact]
        async public Task ExecuteAsync_MethodInfo_WhenExecuteCommandThrowsCommandError_ShowsHelp()
        {
            var parser = Substitute.For<ICommandParser>();
            parser.Parse().Returns(new CommandPart[0]);

            var options = Substitute.For<ICommandExecutorOptions>();

            var helper = Substitute.For<ICommandHelper>();

            var impl = Substitute.For<ICommandExecutorImpl>();

            var badCommand = new CommandAttribute();
            impl.WhenForAnyArgs(x => x.ExecuteCommandAsync(default, default)).Throw(new BadCommandException(badCommand, default));

            var sut = CreateSut(parser, executorOptions: options, commandHelper: helper, impl: impl);

            var commandMethod = ((Action)Method).GetMethodInfo();
            await sut.ExecuteAsync(commandMethod);

            helper.Received(1).WriteCommandHelp(badCommand, options);
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

            var result = sut.TryAddToLastParameter(null, "hello", new SortedList<int, object>());

            result.Should().BeFalse();
        }

        [Fact]
        public void TryAddToLastParameter_WhenLastParameterNotArray_ReturnsFalse()
        {
            var list = new SortedList<int, object>();
            var parameter = new TestParameterInfo("test", typeof(string));

            var sut = CreateSutImpl();

            var result = sut.TryAddToLastParameter(parameter, "hello", list);

            result.Should().BeFalse();
            list.Should().BeEmpty();
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

            var result = sut.TryAddToLastParameter(parameter, value, parameters);

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

            var result = sut.TryAddToLastParameter(parameter, value, parameters);

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

            var result = sut.TryAddToLastParameter(parameter, value, parameters);

            result.Should().BeTrue();
            parameters.Count.Should().Be(1);
            parameters[position].Should().BeEquivalentTo(new string[] { existingValue[0], existingValue[1], value });
        }

        [Fact]
        async public Task ExecuteCommandAsync_CallsSetParameterForEachCommandPart()
        {
            var command = new CommandAttribute();
            var commandParts = new List<CommandPart>(new[]
            {
                new CommandPart(),
                new CommandPart(),
                new CommandPart()
            });

            var key1 = "Hello";
            var key2 = "World";
            var key3 = "Peeps";

            var impl = Substitute.For<ICommandExecutorImpl>();

            var parser = Substitute.For<ICommandParser>();
            parser.GetString(default).ReturnsForAnyArgs(key1, key2, key3);

            var sut = CreateSutImpl(impl: impl, parser: parser);

            await sut.ExecuteCommandAsync(command, commandParts.GetEnumerator());

            impl.Received(1).SetValueParameter(key1, command, Arg.Any<SortedList<int, object>>(), Arg.Any<ParameterInfo>());
            impl.Received(1).SetValueParameter(key2, command, Arg.Any<SortedList<int, object>>(), Arg.Any<ParameterInfo>());
            impl.Received(1).SetValueParameter(key3, command, Arg.Any<SortedList<int, object>>(), Arg.Any<ParameterInfo>());
        }

        [Fact]
        async public Task ExecuteCommandAsync_CallsAddDefaultValuesWithCommand()
        {
            var command = new CommandAttribute();
            var commandParts = new List<CommandPart>();

            var impl = Substitute.For<ICommandExecutorImpl>();

            var sut = CreateSutImpl(impl: impl);

            await sut.ExecuteCommandAsync(command, commandParts.GetEnumerator());

            impl.ReceivedWithAnyArgs(1).AddDefaultValues(command, Arg.Any<SortedList<int, object>>());
        }

        [Fact]
        async public Task ExecuteCommandAsync_ExecutesCommand()
        {
            var command = new CommandAttribute();
            var commandParts = new List<CommandPart>();

            var impl = Substitute.For<ICommandExecutorImpl>();

            var sut = CreateSutImpl(impl: impl);

            await sut.ExecuteCommandAsync(command, commandParts.GetEnumerator());

            await impl.Received(1).ExecuteActualCommandAsync(command, Arg.Any<SortedList<int, object>>());
        }

        [Fact]
        public void SetValueParameter_NoLastParameter_AddsToParameters()
        {
            var command = new CommandAttribute();
            var parameters = new SortedList<int, object>();
            var key = "Hello World";
            var parameterType = typeof(Random);
            var parameter = new TestParameterInfo(type: parameterType);
            var value = new object();

            command.Values.Add(new ValueAttribute
            {
                Parameter = parameter
            });

            var valueConverter = Substitute.For<IValueConverter>();
            valueConverter.GetValue(key, parameterType).Returns(value);

            var sut = CreateSutImpl(valueConverter: valueConverter);

            var result = sut.SetValueParameter(key, command, parameters, null);

            result.Should().Be(parameter);
            parameters.Count.Should().Be(1);
            parameters[0].Should().Be(value);
        }

        [Fact]
        public void SetValueParameter_NoMoreValues_Throws()
        {
            var command = new CommandAttribute();
            var parameters = new SortedList<int, object>();
            var parameter = new TestParameterInfo();

            var sut = CreateSutImpl();

            Action act = () => sut.SetValueParameter("", command, parameters, null);

            act.Should().Throw<BadCommandException>();
        }

        [Fact]
        public void SetValueParameter_AddsToLastParameter_ReturnsLastParameter()
        {
            var command = new CommandAttribute();
            var parameters = new SortedList<int, object>();
            var parameter = new TestParameterInfo();

            var impl = Substitute.For<ICommandExecutorImpl>();
            impl.TryAddToLastParameter(default, default, default).ReturnsForAnyArgs(true);

            var sut = CreateSutImpl(impl: impl);

            var result = sut.SetValueParameter("", command, parameters, parameter);

            result.Should().Be(parameter);
        }

        [Fact]
        public void SetOptionParameter_OptionDoesntExist_Throws()
        {
            var commandPart = new CommandPart();
            var command = new CommandAttribute();
            var parameters = new SortedList<int, object>();

            var sut = CreateSutImpl();

            Action act = () => sut.SetOptionParameter("", commandPart, command, parameters);

            act.Should().Throw<BadCommandException>();
        }

        [Fact]
        public void SetOptionParameter_ParameterIsNotArray_AndAlreadyExists_Throws()
        {
            var commandPart = new CommandPart();
            var command = new CommandAttribute();
            var parameters = new SortedList<int, object>();
            var position = 10;

            parameters.Add(position, new object());

            var key = "Hello World";

            command.Options.Add(key, new OptionAttribute
            {
                Parameter = new TestParameterInfo(type: typeof(object), position: position)
            });

            var sut = CreateSutImpl();

            Action act = () => sut.SetOptionParameter(key, commandPart, command, parameters);

            act.Should().Throw<BadCommandException>();
        }

        [Fact]
        public void SetOptionParameter_DoesntExist_AddsToParameters()
        {
            var commandPart = new CommandPart();
            var command = new CommandAttribute();
            var parameters = new SortedList<int, object>();
            var position = 10;
            var value = new object();
            var parameter = new TestParameterInfo(type: typeof(object), position: position);

            var key = "Hello World";

            command.Options.Add(key, new OptionAttribute
            {
                Parameter = parameter
            });

            var valueConverter = Substitute.For<IValueConverter>();
            valueConverter.GetValue(default, default).ReturnsForAnyArgs(value);

            var sut = CreateSutImpl(valueConverter: valueConverter);

            var result = sut.SetOptionParameter(key, commandPart, command, parameters);

            result.Should().Be(parameter);

            parameters[position].Should().Be(value);
        }

        [Fact]
        public void SetOptionParameter_IsArray_AndExist_AppendsToExistingValue()
        {
            var commandPart = new CommandPart();
            var command = new CommandAttribute();
            var parameters = new SortedList<int, object>();
            var position = 10;
            var newValue = new[] { new object(), new object() };
            var existingValue = new[] { new object(), new object() };
            var parameter = new TestParameterInfo(type: typeof(object[]), position: position);

            var key = "Hello World";

            parameters.Add(position, existingValue);

            command.Options.Add(key, new OptionAttribute
            {
                Parameter = parameter
            });

            var valueConverter = Substitute.For<IValueConverter>();
            valueConverter.GetValue(default, default).ReturnsForAnyArgs(newValue);

            var sut = CreateSutImpl(valueConverter: valueConverter);

            var result = sut.SetOptionParameter(key, commandPart, command, parameters);

            result.Should().Be(parameter);

            ((Array)parameters[position]).Should().BeEquivalentTo(existingValue.Concat(newValue));
        }
    }
}

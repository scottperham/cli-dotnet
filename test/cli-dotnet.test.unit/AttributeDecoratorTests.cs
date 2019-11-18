using FluentAssertions;
using NSubstitute;
using System.Reflection;
using Xunit;

namespace cli_dotnet.test.unit
{
    public class AttributeDecoratorTests
    {
        public IAttributeDecorator CreateSut(ITypeHelper typeHelper = null, IAttributeDecorator attributeDecoratorImpl = null)
        {
            typeHelper ??= Substitute.For<ITypeHelper>();

            return new AttributeDecorator(typeHelper, attributeDecoratorImpl);
        }

        [Fact]
        public void Decorate_Command_WithValues_AddsToCollection()
        {
            var p1 = new TestParameterInfo { Value = new ValueAttribute() };
            var p2 = new TestParameterInfo { Value = new ValueAttribute() };

            var methodInfo = Substitute.For<MethodInfo>();
            methodInfo.GetParameters().Returns(new[] { p1, p2 });

            var commandAttribute = new CommandAttribute
            {
                Method = methodInfo
            };

            var typeHelper = Substitute.For<ITypeHelper>();
            typeHelper.TryGetValueAttribute(Arg.Any<ParameterInfo>(), out _).ReturnsForAnyArgs(x =>
            {
                x[1] = x.Arg<TestParameterInfo>().Value;
                return x[1] != null;
            });

            var sut = CreateSut(typeHelper);

            sut.Decorate(commandAttribute);

            commandAttribute.Values.Should().BeEquivalentTo(p1.Value, p2.Value);

            commandAttribute.Values[0].Parameter.Should().BeSameAs(p1);
            commandAttribute.Values[1].Parameter.Should().BeSameAs(p2);
            commandAttribute.Options.Should().BeEmpty();
        }

        [Fact]
        public void Decorate_Command_WithOptions_AddsToCollection()
        {
            var p1 = new TestParameterInfo("test1") { Option = new OptionAttribute() };
            var p2 = new TestParameterInfo("test2") { Option = new OptionAttribute() };

            var methodInfo = Substitute.For<MethodInfo>();
            methodInfo.GetParameters().Returns(new[] { p1, p2 });

            var commandAttribute = new CommandAttribute
            {
                Method = methodInfo
            };

            var typeHelper = Substitute.For<ITypeHelper>();
            typeHelper.TryGetOptionAttribute(Arg.Any<ParameterInfo>(), out _).ReturnsForAnyArgs(x =>
            {
                x[1] = x.Arg<TestParameterInfo>().Option;
                return x[1] != null;
            });

            var sut = CreateSut(typeHelper);

            sut.Decorate(commandAttribute);

            commandAttribute.Options.Keys.Should().BeEquivalentTo(p1.Name, p2.Name);
            commandAttribute.Options.Values.Should().BeEquivalentTo(p1.Option, p2.Option);
            commandAttribute.Values.Should().BeEmpty();
        }

        [Fact]
        public void Decorate_Command_WithNamedOptions_AddsToCollection()
        {
            var p1 = new TestParameterInfo("test1") { Option = new OptionAttribute(longForm: "longtest1") };
            var p2 = new TestParameterInfo("test2") { Option = new OptionAttribute(longForm: "longtest2") };

            var methodInfo = Substitute.For<MethodInfo>();
            methodInfo.GetParameters().Returns(new[] { p1, p2 });

            var commandAttribute = new CommandAttribute
            {
                Method = methodInfo
            };

            var typeHelper = Substitute.For<ITypeHelper>();
            typeHelper.TryGetOptionAttribute(Arg.Any<ParameterInfo>(), out _).ReturnsForAnyArgs(x =>
            {
                x[1] = x.Arg<TestParameterInfo>().Option;
                return x[1] != null;
            });

            var sut = CreateSut(typeHelper);

            sut.Decorate(commandAttribute);

            commandAttribute.Options.Keys.Should().BeEquivalentTo(p1.Option.LongForm, p2.Option.LongForm);
            commandAttribute.Options.Values.Should().BeEquivalentTo(p1.Option, p2.Option);
            commandAttribute.Values.Should().BeEmpty();
        }

        [Fact]
        public void Decorate_Command_WithSFOptions_AddsToCollection()
        {
            var p1 = new TestParameterInfo("test1") { Option = new OptionAttribute('a') };
            var p2 = new TestParameterInfo("test2") { Option = new OptionAttribute('b') };

            var methodInfo = Substitute.For<MethodInfo>();
            methodInfo.GetParameters().Returns(new[] { p1, p2 });

            var commandAttribute = new CommandAttribute
            {
                Method = methodInfo
            };

            var typeHelper = Substitute.For<ITypeHelper>();
            typeHelper.TryGetOptionAttribute(Arg.Any<ParameterInfo>(), out _).ReturnsForAnyArgs(x =>
            {
                x[1] = x.Arg<TestParameterInfo>().Option;
                return x[1] != null;
            });

            var sut = CreateSut(typeHelper);

            sut.Decorate(commandAttribute);

            commandAttribute.Options.Keys.Should().BeEquivalentTo(p1.Name, p1.Option.ShortForm.ToString(), p2.Name, p2.Option.ShortForm.ToString());
            commandAttribute.Options.Values.Should().BeEquivalentTo(p1.Option, p1.Option, p2.Option, p2.Option);
            commandAttribute.Values.Should().BeEmpty();
        }

        [Fact]
        public void Decorate_Command_WithSFAndLFOptions_AddsToCollection()
        {
            var p1 = new TestParameterInfo("test1") { Option = new OptionAttribute('a', "a1") };
            var p2 = new TestParameterInfo("test2") { Option = new OptionAttribute('b', "a2") };

            var methodInfo = Substitute.For<MethodInfo>();
            methodInfo.GetParameters().Returns(new[] { p1, p2 });

            var commandAttribute = new CommandAttribute
            {
                Method = methodInfo
            };

            var typeHelper = Substitute.For<ITypeHelper>();
            typeHelper.TryGetOptionAttribute(Arg.Any<ParameterInfo>(), out _).ReturnsForAnyArgs(x =>
            {
                x[1] = x.Arg<TestParameterInfo>().Option;
                return x[1] != null;
            });

            var sut = CreateSut(typeHelper);

            sut.Decorate(commandAttribute);

            commandAttribute.Options.Keys.Should().BeEquivalentTo(p1.Option.LongForm, p1.Option.ShortForm.ToString(), p2.Option.LongForm, p2.Option.ShortForm.ToString());
            commandAttribute.Options.Values.Should().BeEquivalentTo(p1.Option, p1.Option, p2.Option, p2.Option);
            commandAttribute.Values.Should().BeEmpty();
        }

        [Fact]
        public void Decorate_Command_WithoutOptionsOrValue_AddsToCollection()
        {
            var p1 = new TestParameterInfo("test1");
            var p2 = new TestParameterInfo("test2");

            var methodInfo = Substitute.For<MethodInfo>();
            methodInfo.GetParameters().Returns(new[] { p1, p2 });

            var commandAttribute = new CommandAttribute
            {
                Method = methodInfo
            };

            var typeHelper = Substitute.For<ITypeHelper>();
            typeHelper.TryGetOptionAttribute(Arg.Any<ParameterInfo>(), out _).ReturnsForAnyArgs(x =>
            {
                x[1] = x.Arg<TestParameterInfo>().Option;
                return x[1] != null;
            });

            var sut = CreateSut(typeHelper);

            sut.Decorate(commandAttribute);

            commandAttribute.Options.Keys.Should().BeEquivalentTo(p1.Name, p2.Name);
            commandAttribute.Options[p1.Name].Parameter.Should().BeSameAs(p1);
            commandAttribute.Options[p2.Name].Parameter.Should().BeSameAs(p2);
            commandAttribute.Values.Should().BeEmpty();
        }

        [Fact]
        public void Decorate_Command_WithOneOfEach_AddsToCollection()
        {
            var p1 = new TestParameterInfo("test1") { Value = new ValueAttribute() };
            var p2 = new TestParameterInfo("test2") { Option = new OptionAttribute() };
            var p3 = new TestParameterInfo("test3");

            var methodInfo = Substitute.For<MethodInfo>();
            methodInfo.GetParameters().Returns(new[] { p1, p2, p3 });

            var commandAttribute = new CommandAttribute
            {
                Method = methodInfo
            };

            var typeHelper = Substitute.For<ITypeHelper>();
            typeHelper.TryGetValueAttribute(Arg.Any<ParameterInfo>(), out _).ReturnsForAnyArgs(x =>
            {
                x[1] = x.Arg<TestParameterInfo>().Value;
                return x[1] != null;
            });
            typeHelper.TryGetOptionAttribute(Arg.Any<ParameterInfo>(), out _).ReturnsForAnyArgs(x =>
            {
                x[1] = x.Arg<TestParameterInfo>().Option;
                return x[1] != null;
            });

            var sut = CreateSut(typeHelper);

            sut.Decorate(commandAttribute);

            commandAttribute.Values.Should().BeEquivalentTo(p1.Value);
            p1.Value.Parameter.Should().Be(p1);

            commandAttribute.Options.Keys.Should().BeEquivalentTo(p2.Name, p3.Name);
            commandAttribute.Options[p2.Name].Parameter.Should().BeSameAs(p2);
            commandAttribute.Options[p3.Name].Parameter.Should().BeSameAs(p3);
        }

        [Fact]
        public void Decorate_Verb_WithValidProperties_AddsToDictionary()
        {
            var rootVerb = new VerbAttribute
            {
                Instance = new object()
            };

            var mi1Instance = new object();
            var mi1 = Substitute.For<PropertyInfo>();
            mi1.Name.Returns("test1");
            mi1.GetValue(rootVerb.Instance).Returns(mi1Instance);
            var mi1Verb = new VerbAttribute();

            var mi2Instance = new object();
            var mi2 = Substitute.For<PropertyInfo>();
            mi2.Name.Returns("test2");
            mi2.GetValue(rootVerb.Instance).Returns(mi2Instance);
            var mi2Verb = new VerbAttribute();

            var typeHelper = Substitute.For<ITypeHelper>();
            typeHelper.GetPropertiesAndMethods(rootVerb.Instance).Returns(new[] { mi1, mi2 });
            typeHelper.TryGetVerbAttribute(default, out _).ReturnsForAnyArgs(
                x => { x[1] = mi1Verb; return true; },
                x => { x[1] = mi2Verb; return true; });

            var impl = Substitute.For<IAttributeDecorator>();

            var sut = CreateSut(typeHelper, impl);

            sut.Decorate(rootVerb);

            impl.Received(1).Decorate(mi1Verb);
            impl.Received(1).Decorate(mi2Verb);

            rootVerb.Verbs.Keys.Should().BeEquivalentTo("test1", "test2");
            rootVerb.Verbs.Values.Should().BeEquivalentTo(mi1Verb, mi2Verb);
            mi1Verb.Instance.Should().BeSameAs(mi1Instance);
            mi1Verb.ParentVerb.Should().BeSameAs(rootVerb);
            mi1Verb.Property.Should().BeSameAs(mi1);
            mi2Verb.Instance.Should().BeSameAs(mi2Instance);
            mi2Verb.ParentVerb.Should().BeSameAs(rootVerb);
            mi2Verb.Property.Should().BeSameAs(mi2);
        }

        [Fact]
        public void Decorate_Verb_WithValidMethods_AddsToDictionary()
        {
            var rootVerb = new VerbAttribute
            {
                Instance = new object()
            };

            var mi1Instance = new object();
            var mi1 = Substitute.For<MethodInfo>();
            mi1.Name.Returns("test1");
            var m1Command = new CommandAttribute();

            var mi2Instance = new object();
            var mi2 = Substitute.For<MethodInfo>();
            mi2.Name.Returns("test2");
            var m2Command = new CommandAttribute();

            var typeHelper = Substitute.For<ITypeHelper>();
            typeHelper.GetPropertiesAndMethods(rootVerb.Instance).Returns(new[] { mi1, mi2 });
            typeHelper.TryGetCommandAttribute(default, out _).ReturnsForAnyArgs(
                x => { x[1] = m1Command; return true; },
                x => { x[1] = m2Command; return true; });

            var impl = Substitute.For<IAttributeDecorator>();

            var sut = CreateSut(typeHelper, impl);

            sut.Decorate(rootVerb);

            impl.Received(1).Decorate(m1Command);
            impl.Received(1).Decorate(m2Command);

            rootVerb.Commands.Keys.Should().BeEquivalentTo("test1", "test2");
            rootVerb.Commands.Values.Should().BeEquivalentTo(m1Command, m2Command);
            m1Command.ParentVerb.Should().BeSameAs(rootVerb);
            m1Command.Method.Should().BeSameAs(mi1);
            m2Command.ParentVerb.Should().BeSameAs(rootVerb);
            m2Command.Method.Should().BeSameAs(mi2);
        }

        [Fact]
        public void Decorate_Verb_WithValidMethodsAndProperties_AddsToDictionary()
        {
            var rootVerb = new VerbAttribute
            {
                Instance = new object()
            };

            var mi1Instance = new object();
            var mi1 = Substitute.For<PropertyInfo>();
            mi1.Name.Returns("test1");
            mi1.GetValue(rootVerb.Instance).Returns(mi1Instance);
            var mi1Verb = new VerbAttribute();

            var mi2Instance = new object();
            var mi2 = Substitute.For<MethodInfo>();
            mi2.Name.Returns("test2");
            var m2Command = new CommandAttribute();

            var typeHelper = Substitute.For<ITypeHelper>();
            typeHelper.GetPropertiesAndMethods(rootVerb.Instance).Returns(new MemberInfo[] { mi1, mi2 });
            typeHelper.TryGetVerbAttribute(default, out _).ReturnsForAnyArgs(x => { x[1] = mi1Verb; return true; });
            typeHelper.TryGetCommandAttribute(default, out _).ReturnsForAnyArgs(x => { x[1] = m2Command; return true; });

            var impl = Substitute.For<IAttributeDecorator>();

            var sut = CreateSut(typeHelper, impl);

            sut.Decorate(rootVerb);

            impl.Received(1).Decorate(mi1Verb);
            impl.Received(1).Decorate(m2Command);

            rootVerb.Verbs.Keys.Should().BeEquivalentTo("test1");
            rootVerb.Verbs.Values.Should().BeEquivalentTo(mi1Verb);
            mi1Verb.Instance.Should().BeSameAs(mi1Instance);
            mi1Verb.ParentVerb.Should().BeSameAs(rootVerb);
            mi1Verb.Property.Should().BeSameAs(mi1);

            rootVerb.Commands.Keys.Should().BeEquivalentTo("test2");
            rootVerb.Commands.Values.Should().BeEquivalentTo(m2Command);
            m2Command.ParentVerb.Should().BeSameAs(rootVerb);
            m2Command.Method.Should().BeSameAs(mi2);
        }
    }
}

using FluentAssertions;
using FluentAssertions.Equivalency;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace cli_dotnet.test.unit
{
    public class ValueConverterTests
    {
        [Fact]
        public void GetValue_ReturnsCorrectValue()
        {
            var tests = new (string value, Type type, object result)[]
            {
                ("", typeof(string), ""),
                ("123", typeof(string), "123"),
                ("true", typeof(string), "true"),
                ("1", typeof(string), "1"),
                ("NotStrict", typeof(string), "NotStrict"),

                ("1", typeof(string[]), new[] { "1" }),

                ("123", typeof(int), 123),
                ("-123", typeof(int), -123),
                ("1", typeof(int[]), new[] { 1 }),

                ("123", typeof(long), 123L),
                ("-123", typeof(long), -123L),
                ("1", typeof(long[]), new[] { 1L }),

                ("FALSE", typeof(bool), false),
                ("TrUe", typeof(bool), true),
                ("true", typeof(bool[]), new[] { true }),

                ("1", typeof(OrderStrictness), OrderStrictness.NotStrict),
                ("notstrict", typeof(OrderStrictness), OrderStrictness.NotStrict),
                ("1", typeof(OrderStrictness[]), new[] { OrderStrictness.NotStrict }),
            };

            var sut = (IValueConverter)new ValueConverter();

            foreach(var (value, type, result) in tests)
            {
                var actual = sut.GetValue(value, type);

                actual.Should().BeEquivalentTo(result);
            }
        }
        [Fact]
        public void GetDefaultValue_ReturnsCorrectValue()
        {
            var tests = new (Type type, object result)[]
            {
                (typeof(string), ""),
                (typeof(string[]), new string[0]),

                (typeof(int), 0),
                (typeof(int[]), new int[0]),

                (typeof(long), 0L),
                (typeof(long[]), new long[0]),

                (typeof(bool), false),
                (typeof(bool[]), new bool[0]),

                (typeof(OrderStrictness), OrderStrictness.Strict),
                (typeof(OrderStrictness[]), new OrderStrictness[0]),
            };

            var sut = (IValueConverter)new ValueConverter();

            foreach (var (type, result) in tests)
            {
                var actual = sut.CreateDefaultValue(type);

                actual.Should().BeEquivalentTo(result);
            }
        }
    }
}

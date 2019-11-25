using System;
using System.Reflection;

namespace cli_dotnet.test.unit
{
    public class TestParameterInfo : ParameterInfo
    {
        readonly Type _type;
        readonly int _position;

        public TestParameterInfo(string name = null, Type type = null, int position = 0)
        {
            NameImpl = name ?? "";
            _type = type;
            _position = position;
        }

        public OptionAttribute Option { get; set; }
        public ValueAttribute Value { get; set; }

        public override Type ParameterType => _type;
        public override int Position => _position;
    }
}

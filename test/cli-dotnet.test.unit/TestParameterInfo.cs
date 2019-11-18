using System.Reflection;

namespace cli_dotnet.test.unit
{
    public class TestParameterInfo : ParameterInfo
    { 
        public TestParameterInfo(string name = null)
        {
            NameImpl = name ?? "";
        }

        public OptionAttribute Option { get; set; }
        public ValueAttribute Value { get; set; }
    }
}

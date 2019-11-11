using System;
using System.Reflection;

namespace CLI
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public abstract class ParameterAttribute : Attribute
    {
        private readonly string _name;

        public ParameterAttribute(string name)
        {
            _name = name;
        }

        internal ParameterInfo Parameter { get; set; }
        internal string GetName() => _name ?? Parameter.Name;
    }
}

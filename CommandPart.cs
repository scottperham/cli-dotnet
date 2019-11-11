namespace CLI
{
    public struct CommandPart
    {
        public bool IsArgument;
        public bool IsShortForm;
        public StringReference Key;
        public StringReference Value;
    }
}

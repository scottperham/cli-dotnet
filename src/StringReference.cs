namespace cli_dotnet
{
    public struct StringReference
    {
        public StringReference(int start, int length)
        {
            Start = start;
            Length = length;
        }

        public int Start;
        public int Length;

        public bool IsEmpty() => Start == 0 && Length == 0;
    }
}

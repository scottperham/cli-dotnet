namespace cli_dotnet
{
    public struct StringReference
    {
        public int Length;
        public int Start;

        public bool IsEmpty() => Start == 0 && Length == 0;
    }
}

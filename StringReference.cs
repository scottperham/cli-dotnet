namespace CLI
{
    public struct StringReference
    {
        public int Start;
        public int Length;

        public bool IsEmpty() => Start == 0 && Length == 0;
    }
}

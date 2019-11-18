namespace cli_dotnet
{
    public interface ICommandParserImplementation
    {
        char Peek();
        void Consume();
        bool ConsumeIf(char ch);
        void ConsumeWhitespace();

        int Position { get; set; }
        string Command { get; set; }

        StringReference ScanTo(char ch, char escape = '\0');
        StringReference ScanToAny(params char[] scanTo);
        bool TryGetCommandPart(out CommandPart commandPart);

        CommandPart GetShortFormArgument();
        CommandPart GetLongFormArgument();
        CommandPart GetValue();
        StringReference GetValueReference(bool allowQuotes);
    }
}

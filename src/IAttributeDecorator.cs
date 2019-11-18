namespace cli_dotnet
{
    public interface IAttributeDecorator
    {
        void Decorate(CommandAttribute commandAttribute);
        void Decorate(VerbAttribute verbAttribute);
    }
}

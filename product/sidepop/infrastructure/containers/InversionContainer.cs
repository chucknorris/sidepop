namespace sidepop.infrastructure.containers
{
    public interface InversionContainer
    {
        TypeToReturn Resolve<TypeToReturn>();
    }
}
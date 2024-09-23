namespace HeyaChat_Authorization.Repositories.Interfaces
{
    public interface ISuspensionsRepository
    {
        bool IsCurrentlySuspended(long userId);
    }
}

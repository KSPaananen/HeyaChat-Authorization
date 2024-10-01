namespace HeyaChat_Authorization.Repositories.Interfaces
{
    public interface ISuspensionsRepository
    {
        (bool suspended, bool permanent) IsCurrentlySuspended(long userId);
    }
}

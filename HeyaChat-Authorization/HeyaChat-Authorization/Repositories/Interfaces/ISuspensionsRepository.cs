using HeyaChat_Authorization.Models;

namespace HeyaChat_Authorization.Repositories.Interfaces
{
    public interface ISuspensionsRepository
    {
        Suspension IsCurrentlySuspended(long userId);
    }
}

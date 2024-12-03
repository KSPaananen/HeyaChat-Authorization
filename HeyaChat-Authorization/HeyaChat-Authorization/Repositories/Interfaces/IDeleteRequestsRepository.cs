using HeyaChat_Authorization.Models;

namespace HeyaChat_Authorization.Repositories.Interfaces
{
    public interface IDeleteRequestsRepository
    {
        DeleteRequest GetDeleteRequestByUserId(long userId);

        long InsertDeleteRequest(DeleteRequest request);

        void DeleteDeleteRequest(long userId);
    }
}

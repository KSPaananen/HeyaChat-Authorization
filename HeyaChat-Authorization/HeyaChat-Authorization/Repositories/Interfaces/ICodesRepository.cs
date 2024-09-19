using HeyaChat_Authorization.Models;

namespace HeyaChat_Authorization.Repositories.Interfaces
{
    public interface ICodesRepository
    {
        // Returns ID of a created row
        long InsertCode(long userId, string code);

        Codes IsCodeValid(long userId, string code);
    }
}

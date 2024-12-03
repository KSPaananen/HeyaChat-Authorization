using HeyaChat_Authorization.Models;

namespace HeyaChat_Authorization.Repositories.Interfaces
{
    public interface ICodesRepository
    {
        // Returns object of the found row
        Codes GetCodeByUserId(long userId);

        // Returns code object found with userId and code valid expiration timespan
        Codes GetValidCodeWithUserIdAndCode(long userId, string code);

        // Returns ID of a created row
        long InsertCode(Codes code);

        // Returns ID of the affected row
        long UpdateCode(Codes code);
    }
}

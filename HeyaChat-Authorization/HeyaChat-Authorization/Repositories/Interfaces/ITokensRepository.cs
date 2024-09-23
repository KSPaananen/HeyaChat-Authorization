using HeyaChat_Authorization.DataObjects.DRO.SubClasses;
using HeyaChat_Authorization.Models;
using Microsoft.AspNetCore.Http.HttpResults;

namespace HeyaChat_Authorization.Repositories.Interfaces
{
    public interface ITokensRepository
    {
        // Returns object of the row found
        Token GetTokenByGuid(Guid identifier);

        // Returns ID of a created row
        long InsertToken(Token token);

        // Returns ID of a the affected row
        long UpdateToken(Token token);

        void InvalidateAllTokens(long deviceId);

        // Returns row if token is valid. Returns new object if not.
        Token IsTokenValid(Guid jti, UserDevice device);
    }
}

using HeyaChat_Authorization.DataObjects.DRO.SubClasses;
using HeyaChat_Authorization.Models;

namespace HeyaChat_Authorization.Repositories.Interfaces
{
    public interface ITokensRepository
    {
        // Returns ID of a created row
        long InsertToken(Token token);

        long InvalidateToken(Guid identifier);

        // Returns row if token is valid. Returns new object if not.
        Token IsTokenValid(Guid jti, UserDevice device);
    }
}

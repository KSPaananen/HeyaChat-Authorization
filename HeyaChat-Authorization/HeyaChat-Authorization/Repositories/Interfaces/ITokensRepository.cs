using HeyaChat_Authorization.DataObjects.DRO.SubClasses;
using HeyaChat_Authorization.Models;

namespace HeyaChat_Authorization.Repositories.Interfaces
{
    public interface ITokensRepository
    {
        // Returns ID of a created row
        long InsertToken(Token token);

        // Returns boolean based on if row can be found
        bool IsTokenValid(Guid jti, UserDevice device);
    }
}

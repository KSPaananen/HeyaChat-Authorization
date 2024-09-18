using HeyaChat_Authorization.DataObjects.DRO.SubClasses;
using HeyaChat_Authorization.Models;
using HeyaChat_Authorization.Models.Context;
using HeyaChat_Authorization.Repositories.Interfaces;

namespace HeyaChat_Authorization.Repositories
{
    public class TokensRepository : ITokensRepository
    {
        private AuthorizationDBContext _context;

        public TokensRepository(AuthorizationDBContext context)
        {
            _context = context ?? throw new NullReferenceException(nameof(context));
        }

        public long InsertToken(Token token)
        {
            try
            {
                _context.Tokens.Add(token);
                _context.SaveChanges();

                return token.TokenId;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public bool IsTokenValid(Guid jti, UserDevice deviceDetails)
        {
            try
            {
                int tokenCount = (from device in _context.Devices
                                  join token in _context.Tokens on device.DeviceId equals token.DeviceId
                                  where device.DeviceIdentifier == deviceDetails.DeviceIdentifier && token.Identifier == jti && token.ExpiresAt > DateTime.UtcNow && token.Active == true
                                  select token).Count();

                if (tokenCount > 0)
                {
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}

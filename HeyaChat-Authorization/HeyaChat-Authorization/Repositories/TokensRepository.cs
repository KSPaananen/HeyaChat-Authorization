using HeyaChat_Authorization.DataObjects.DRO.SubClasses;
using HeyaChat_Authorization.Models;
using HeyaChat_Authorization.Models.Context;
using HeyaChat_Authorization.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HeyaChat_Authorization.Repositories
{
    public class TokensRepository : ITokensRepository
    {
        private AuthorizationDBContext _context;

        public TokensRepository(AuthorizationDBContext context)
        {
            _context = context ?? throw new NullReferenceException(nameof(context));
        }

        public Token GetTokenByGuid(Guid identifier)
        {
            try
            {
                var result = (from tokens in _context.Tokens
                              where tokens.Identifier == identifier
                              select tokens).FirstOrDefault() ?? new Token();

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
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

        public long UpdateToken(Token token)
        {
            try
            {
                _context.Attach(token);
                _context.Entry(token).State = EntityState.Modified;
                int affectedRows = _context.SaveChanges();

                if (affectedRows <= 0)
                {
                    throw new Exception($"Token with the ID {token.TokenId} could not be updated.");
                }

                return token.TokenId;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public void InvalidateAllTokens(long userId)
        {
            try
            {
                var results = (from devices in _context.Devices
                               join tokens in _context.Tokens on devices.DeviceId equals tokens.DeviceId
                               where devices.UserId == userId && tokens.DeviceId == devices.DeviceId && tokens.Active == true
                               select tokens).ToArray();

                foreach (var items in results)
                {
                    items.Active = false;
                }

                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public Token IsTokenValid(Guid jti, UserDevice deviceDetails)
        {
            try
            {
                var result = (from device in _context.Devices
                                  join token in _context.Tokens on device.DeviceId equals token.DeviceId
                                  where device.DeviceIdentifier == deviceDetails.DeviceIdentifier && token.Identifier == jti && token.ExpiresAt > DateTime.UtcNow && token.Active == true
                                  select token).FirstOrDefault() ?? null;

                if (result != null)
                {
                    return result;
                }

                return new Token();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


    }
}

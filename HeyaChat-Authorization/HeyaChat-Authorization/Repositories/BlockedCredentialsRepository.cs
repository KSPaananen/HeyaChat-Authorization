using HeyaChat_Authorization.Models;
using HeyaChat_Authorization.Models.Context;
using HeyaChat_Authorization.Repositories.Interfaces;

namespace HeyaChat_Authorization.Repositories
{
    public class BlockedCredentialsRepository : IBlockedCredentialsRepository
    {
        private AuthorizationDBContext _context;

        public BlockedCredentialsRepository(AuthorizationDBContext context)
        {
            _context = context ?? throw new NullReferenceException(nameof(context));
        }

        public bool IsCredentialBlocked(string credential)
        {
            try
            {
                int count = (from cred in _context.BlockedCredentials
                            where cred.Email == credential || cred.Phone == credential
                            select cred).Count();

                if (count > 0 )
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

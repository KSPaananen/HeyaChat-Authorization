using HeyaChat_Authorization.Models.Context;
using HeyaChat_Authorization.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HeyaChat_Authorization.Repositories
{
    public class SuspensionsRepository : ISuspensionsRepository
    {
        private AuthorizationDBContext _context;

        public SuspensionsRepository(AuthorizationDBContext context)
        {
            _context = context ?? throw new NullReferenceException(nameof(context));
        }

        public bool IsCurrentlySuspended(long userId)
        {
            try
            {
                string sql = $"SELECT COUNT(*) FROM suspensions WHERE (expires_at IS NULL OR expires_at > CURRENT_TIMESTAMP) AND lifted_at IS NULL AND user_id = {userId}";

                int count = _context.Database.ExecuteSqlRaw(sql);

                if (count > 0)
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

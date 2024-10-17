using HeyaChat_Authorization.Models;
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

        public Suspension IsCurrentlySuspended(long userId)
        {
            try
            {
                Suspension result = (from suspension in _context.Suspensions
                                     where (suspension.ExpiresAt > DateTime.UtcNow || suspension.ExpiresAt == null) && suspension.LiftedAt == null && suspension.UserId == userId
                                     select suspension).FirstOrDefault() ?? new Suspension();

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}

using HeyaChat_Authorization.Models;
using HeyaChat_Authorization.Models.Context;
using HeyaChat_Authorization.Repositories.Interfaces;

namespace HeyaChat_Authorization.Repositories
{
    /// <summary>
    ///     <para>This table is indexed with UserId.</para>
    /// </summary>
    public class UserDetailsRepository : IUserDetailsRepository
    {
        private AuthorizationDBContext _context;

        public UserDetailsRepository(AuthorizationDBContext context)
        {
            _context = context ?? throw new NullReferenceException(nameof(context));
        }

        public long InsertUserDetailsToTable(UserDetail details)
        {
            try
            {
                _context.UserDetails.Add(details);
                _context.SaveChanges();

                return details.UserId;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public bool DeleteUserDetails(long userId)
        {
            try
            {
                var result = (from user in _context.UserDetails
                              where user.UserId == userId
                              select user).SingleOrDefault() ?? null;

                if (result != null)
                {
                    _context.UserDetails.Remove(result);
                    _context.SaveChanges();

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

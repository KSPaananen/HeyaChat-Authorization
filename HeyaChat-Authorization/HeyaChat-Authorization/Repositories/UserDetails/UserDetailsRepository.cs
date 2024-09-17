using HeyaChat_Authorization.Repositories.UserDetails.Interfaces;
using HeyaChat_Authorization.Models;
using HeyaChat_Authorization.Models.Context;

namespace HeyaChat_Authorization.Repositories.UserDetails
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

        /// <summary>
        ///     Insert a new row to user_details table.
        /// </summary>
        /// <returns>UserId of the created row. 0 if row could not be inserted.</returns>
        public long InsertUserDetailsToTable(UserDetail details)
        {
            try
            {
                _context.UserDetails.Add(details);
                _context.SaveChanges();

                return details.UserId;
            }
            catch
            {
                return 0;
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
            catch
            {
                return false;
            }
        }





    }
}

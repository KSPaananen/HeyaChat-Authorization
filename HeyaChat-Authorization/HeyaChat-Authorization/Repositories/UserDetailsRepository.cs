using HeyaChat_Authorization.Models;
using HeyaChat_Authorization.Models.Context;
using HeyaChat_Authorization.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

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

        public UserDetail GetUserDetailsByUserId(long userId)
        {
            try
            {
                var result = (from detail  in _context.UserDetails
                              where detail.UserId == userId
                              select detail).FirstOrDefault() ?? new UserDetail();

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public long InsertUserDetails(UserDetail detail)
        {
            try
            {
                _context.UserDetails.Add(detail);
                _context.SaveChanges();

                return detail.DetailId;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public long UpdateUserDetails(UserDetail detail)
        {
            try
            {
                _context.Attach(detail);
                _context.Entry(detail).State = EntityState.Modified;
                int affectedRows = _context.SaveChanges();

                if (affectedRows <= 0)
                {
                    throw new Exception($"User details with the ID {detail.DetailId} could not be updated.");
                }

                return detail.DetailId;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


    }
}

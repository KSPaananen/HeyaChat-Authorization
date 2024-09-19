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
                var result = (from details in _context.UserDetails
                              where details.DetailId == detail.DetailId
                              select details).FirstOrDefault() ?? null;

                if (result != null)
                {
                    result = detail;
                    _context.SaveChanges();

                    return result.DetailId;
                }

                return 0;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public long UpdateEmailVerified(long userId)
        {
            try
            {
                var result = (from details in _context.UserDetails
                              where details.UserId == userId
                              select details).FirstOrDefault() ?? null;

                if (result != null)
                {
                    result.EmailVerified = true;
                    _context.SaveChanges();

                    return result.DetailId;
                }

                return 0;
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

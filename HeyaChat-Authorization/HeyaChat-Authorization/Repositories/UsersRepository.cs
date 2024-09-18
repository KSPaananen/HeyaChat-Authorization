using HeyaChat_Authorization.Models;
using HeyaChat_Authorization.Models.Context;
using HeyaChat_Authorization.Repositories.Interfaces;

namespace HeyaChat_Authorization.Repositories
{
    /// <summary>
    ///     <para>This table is indexed with UserId.</para>
    /// </summary>
    public class UsersRepository : IUsersRepository
    {
        private AuthorizationDBContext _context;

        public UsersRepository(AuthorizationDBContext context)
        {
            _context = context ?? throw new NullReferenceException(nameof(context));
        }

        public User GetUserByUserID(long userID)
        {
            try
            {
                var result = (from user in _context.Users
                              where user.UserId == userID
                              select user).SingleOrDefault() ?? new User();

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public long InsertUser(User newUser)
        {
            try
            {
                _context.Users.Add(newUser);
                _context.SaveChanges();

                return newUser.UserId;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public long UpdateUser(User updatedUser)
        {
            try
            {
                var result = (from user in _context.Users
                              where user.UserId == updatedUser.UserId
                              select user).SingleOrDefault() ?? null;

                if (result != null)
                {
                    result = updatedUser;
                    _context.SaveChanges();

                    return result.UserId;
                }

                return 0;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public void DeleteUser(long userID)
        {
            try
            {
                var result = (from user in _context.Users
                              where user.UserId == userID
                              select user).SingleOrDefault() ?? null;

                if (result != null)
                {
                    _context.Users.Remove(result);
                    _context.SaveChanges();
                }

                throw new Exception("Could not find user with provided userId.");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public bool DoesUserExist(string username, string email)
        {
            try
            {
                int userCount = (from user in _context.Users
                                 where user.Username == username || user.Email == email
                                 select user).Count();

                if (userCount > 0)
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

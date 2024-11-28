using HeyaChat_Authorization.Models;
using HeyaChat_Authorization.Models.Context;
using HeyaChat_Authorization.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

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

        public User GetUserByUserID(long userId)
        {
            try
            {
                var result = (from user in _context.Users
                              where user.UserId == userId
                              select user).SingleOrDefault() ?? new User();

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public User GetUserByUsernameOrEmail(string field)
        {
            try
            {
                var result = (from user in _context.Users
                              where user.Username == field || user.Email == field
                              select user).SingleOrDefault() ?? new User();

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public User GetUserByLoginDetails(string login, byte[] biometricsKey)
        {
            try
            {
                var result = (from user in _context.Users
                              where user.Username == login || user.Email == login || user.BiometricsKey == biometricsKey
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
                _context.Attach(updatedUser);
                _context.Entry(updatedUser).State = EntityState.Modified;
                int affectedRows = _context.SaveChanges();

                if (affectedRows <= 0)
                {
                    throw new Exception($"User with the ID {updatedUser.UserId} could not be updated.");
                }

                return updatedUser.UserId;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public (bool usernameInUse, bool emailInUse) UsernameOrEmailInUse(string username, string email)
        {
            try
            {
                User foundUser = (from user in _context.Users
                           where user.Username == username || user.Email == email
                           select user).FirstOrDefault() ?? new User();

                if (foundUser.UserId != 0)
                {
                    bool usernameMatches = false;
                    bool emailMatches = false;

                    if (foundUser.Username == username)
                    {
                        usernameMatches = true;
                    }

                    if (foundUser.Email == email)
                    {
                        emailMatches = true;
                    }

                    return (usernameMatches, emailMatches);
                }

                return (usernameInUse: false, emailInUse: false);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


    }
}

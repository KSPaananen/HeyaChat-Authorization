using HeyaChat_Authorization.Models;
using HeyaChat_Authorization.Models.Context;
using HeyaChat_Authorization.Repositories.UserDetails.Interfaces;
using HeyaChat_Authorization.Repositories.Users.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HeyaChat_Authorization.Repositories.Users
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
            var result = (from user in _context.Users
                          where user.UserId == userID
                          select user).SingleOrDefault() ?? null;

            if (result != null)
            {
                return result;
            }

            throw new InvalidOperationException("Could not find User with UserId");
        }

        /// <summary>
        ///     <para>Check if an existing user can be found with username or email.</para>
        /// </summary>
        /// <returns>True if user with username or email exists. False if no user exists.</returns>
        public bool DoesUserExist(string username, string email)
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

        /// <summary>
        ///     <para>Insert user object to users table.</para>
        /// </summary>
        /// <returns>ID of the created row.</returns>
        public long InsertUser(User newUser)
        {
            try
            {
                _context.Users.Add(newUser);
                _context.SaveChanges();

                return newUser.UserId;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        ///     <para>Delete user from table with UserId.</para>
        /// </summary>
        /// <returns>True if deletion was succesful. False if there was an error.</returns>
        public bool DeleteUser(long userID)
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

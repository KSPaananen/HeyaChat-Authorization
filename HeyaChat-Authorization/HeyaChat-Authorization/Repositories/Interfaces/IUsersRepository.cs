using HeyaChat_Authorization.Models;

namespace HeyaChat_Authorization.Repositories.Interfaces
{
    public interface IUsersRepository
    {
        // Returns user object found with Id
        User GetUserByUserID(long userId);

        // Returns user object found with either username or email
        User GetUserByUsernameOrEmail(string field);

        User GetUserByLoginDetails(string login, byte[] biometricsKey);

        // Returns ID of the created row
        long InsertUser(User newUser);

        // Returns ID of the updated row
        long UpdateUser(User updatedUser);

        // Returns void
        void DeleteUser(long userId);

        // Returns tuple booleans if username already exists or email is blocked from creating another account
        (bool usernameInUse, bool emailInUse) UsernameOrEmailInUse(string username, string email);

    }
}

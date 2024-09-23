using HeyaChat_Authorization.Models;

namespace HeyaChat_Authorization.Repositories.Interfaces
{
    public interface IUsersRepository
    {
        // Returns user object found with ID
        User GetUserByUserID(long userId);

        // Returns user object found with 
        User GetUserByUsernameOrEmail(string field);

        // Returns ID of the created row
        long InsertUser(User newUser);

        // Returns ID of the updated row
        long UpdateUser(User updatedUser);

        // Returns ID of the updated row
        long UpdateUsersPasswordAndSalt(long userId, string passwordHash, byte[] passwordSalt);

        // Returns boolean based on if deletion was succesful
        void DeleteUser(long userID);

        // Returns boolean based on if a row can be found
        bool DoesUserExist(string username, string email);

        // Returns boolean based on if a row can be found
        bool IsLoginValid(string login, string passwordHash);

        // Returns boolean based on if a row can be found
        bool IsBiometricsLoginValid(byte[] biometrics);

    }
}

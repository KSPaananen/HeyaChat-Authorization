using HeyaChat_Authorization.DataObjects.DRO;
using HeyaChat_Authorization.Models;
using Microsoft.AspNetCore.Mvc;

namespace HeyaChat_Authorization.Repositories.Interfaces
{
    public interface IUsersRepository
    {
        // Returns user object found with ID
        User GetUserByUserID(long userID);

        // Returns ID of the created row
        long InsertUser(User newUser);

        // Returns ID of the modified row
        long UpdateUser(User updatedUser);

        // Returns boolean based on if deletion was succesful
        void DeleteUser(long userID);

        // Returns boolean based on if row can be found
        bool DoesUserExist(string username, string email);

    }
}

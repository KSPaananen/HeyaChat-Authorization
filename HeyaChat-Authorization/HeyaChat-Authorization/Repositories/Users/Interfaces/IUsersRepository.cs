using HeyaChat_Authorization.DataObjects.DRO;
using HeyaChat_Authorization.Models;
using Microsoft.AspNetCore.Mvc;

namespace HeyaChat_Authorization.Repositories.Users.Interfaces
{
    public interface IUsersRepository
    {
        User GetUserByUserID(long userID);

        bool DoesUserExist(string username, string email);

        long InsertUser(User newUser);

        bool DeleteUser(long userID);



    }
}

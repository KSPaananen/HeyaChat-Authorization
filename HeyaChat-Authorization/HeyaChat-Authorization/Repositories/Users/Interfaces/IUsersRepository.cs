using HeyaChat_Authorization.DataObjects.DRO;
using HeyaChat_Authorization.Models;
using Microsoft.AspNetCore.Mvc;

namespace HeyaChat_Authorization.Repositories.Users.Interfaces
{
    public interface IUsersRepository
    {
        Models.Users GetUserByUserID(long userID);

        bool DoesUserExist(string username, string email);

        long InsertUser(Models.Users newUser);

        bool DeleteUser(long userID);



    }
}

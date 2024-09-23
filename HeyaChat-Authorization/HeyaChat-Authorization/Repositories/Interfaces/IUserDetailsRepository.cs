using HeyaChat_Authorization.Models;

namespace HeyaChat_Authorization.Repositories.Interfaces
{
    public interface IUserDetailsRepository
    {
        UserDetail GetUserDetailsByUserId(long userId);

        // Returns ID of a created row
        long InsertUserDetails(UserDetail details);

        // Returns ID of the updated row
        long UpdateUserDetails(UserDetail details);

        // Returns boolean based on if deletion was succesful
        bool DeleteUserDetails(long id);

    }
}

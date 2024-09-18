using HeyaChat_Authorization.Models;

namespace HeyaChat_Authorization.Repositories.Interfaces
{
    public interface IUserDetailsRepository
    {
        // Returns ID of a created row
        long InsertUserDetailsToTable(UserDetail details);

        // Returns boolean based on if deletion was succesful
        bool DeleteUserDetails(long id);

    }
}

using HeyaChat_Authorization.Models;

namespace HeyaChat_Authorization.Repositories.UserDetails.Interfaces
{
    public interface IUserDetailsRepository
    {
        long InsertUserDetailsToTable(Models.UserDetails details);

        bool DeleteUserDetails(long id);

    }
}

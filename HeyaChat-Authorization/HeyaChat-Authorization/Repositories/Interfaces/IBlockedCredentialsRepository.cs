namespace HeyaChat_Authorization.Repositories.Interfaces
{
    public interface IBlockedCredentialsRepository
    {
        bool IsCredentialBlocked(string credential);
    }
}

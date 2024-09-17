namespace HeyaChat_Authorization.Repositories.MfaCodes.Interfaces
{
    public interface ICodesRepository
    {
        long InsertCode(long userId, string code);
    }
}

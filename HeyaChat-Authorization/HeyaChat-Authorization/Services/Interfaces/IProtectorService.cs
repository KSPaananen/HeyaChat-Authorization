namespace HeyaChat_Authorization.Services.Interfaces
{
    public interface IProtectorService
    {
        string ProtectData(string data);

        string UnProtectData(string data);
    }
}

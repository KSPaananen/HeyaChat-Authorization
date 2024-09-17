namespace HeyaChat_Authorization.Services.Interfaces
{
    public interface IHasherService
    {
        byte[] GenerateSalt();

        string Hash(string password, byte[] salt);

        bool Verify(byte[] salt, string hash, string passwordString);
    }
}

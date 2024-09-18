namespace HeyaChat_Authorization.Repositories.Interfaces
{
    public interface IConfigurationRepository
    {
        TimeSpan GetTokenLifeTime();

        byte[] GetSigningKey();

        string GetIssuer();

        string GetAudience();

        byte[] GetEncryptionKey();

        string GetConnectionString();

        string GetEmailSender();

        string GetEmailPassword();

        string GetEmailHost();

        int GetEmailPort();

        TimeSpan GetCodeLifeTime();
    }
}

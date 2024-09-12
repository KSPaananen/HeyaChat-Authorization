namespace HeyaChat_Authorization.Repositories.Configuration.Interfaces
{
    public interface IConfigurationRepository
    {
        TimeSpan GetTokenLifeTimeFromConfiguration();

        byte[] GetSigningKeyFromConfiguration();

        string GetIssuerFromConfiguration();

        string GetAudienceFromConfiguration();

        byte[] GetEncryptionKeyFromConfiguration();
    }
}

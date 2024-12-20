﻿namespace HeyaChat_Authorization.Repositories.Interfaces
{
    public interface IConfigurationRepository
    {
        string GetApplicationName();

        TimeSpan GetTokenLifeTime();

        TimeSpan GetTokenRenewTime();

        TimeSpan GetAverageKeyLifetime();

        byte[] GetSigningKey();

        string GetIssuer();

        string GetAudience();

        byte[] GetEncryptionKey();

        string GetAzurePostGreSqlServerConnectionString();

        string GetPostGreSqlServerConnectionString();

        string GetCertificatePath();

        string GetCertificatePassword();

        string GetKeyStoragePath();

        string GetEmailSender();

        string GetEmailPassword();

        string GetEmailHost();

        int GetEmailPort();

        TimeSpan GetCodeLifeTime();

        int GetPermitLimit();

        TimeSpan GetTimeWindow();

        int GetQueueLimit();
    }
}

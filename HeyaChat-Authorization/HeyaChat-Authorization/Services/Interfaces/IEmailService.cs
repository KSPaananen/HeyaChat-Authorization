namespace HeyaChat_Authorization.Services.Interfaces
{
    public interface IEmailService
    {
        void SendVerificationEmail(long userId, string email);

        void SendRecoveryEmail(long userId, string email);
    }
}

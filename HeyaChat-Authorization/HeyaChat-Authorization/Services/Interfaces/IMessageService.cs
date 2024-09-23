namespace HeyaChat_Authorization.Services.Interfaces
{
    public interface IMessageService
    {
        void SendRecoveryEmail(long userId, string email);

        void SendVerificationEmail(long userId, string email);

        void SendVerificationTextMessage(long userId, string email);
    }
}

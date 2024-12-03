namespace HeyaChat_Authorization.Services.Interfaces
{
    public interface IMessageService
    {
        Task SendRecoveryEmail(long userId, string email);

        Task SendVerificationEmail(long userId, string email);

        Task SendVerificationTextMessage(long userId, string email);
    }
}

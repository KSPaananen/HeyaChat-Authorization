namespace HeyaChat_Authorization.Services.Interfaces
{
    public interface IToolsService
    {
        string MaskPhoneNumber(string phoneNumber);

        string MaskEmail(string email);

    }
}

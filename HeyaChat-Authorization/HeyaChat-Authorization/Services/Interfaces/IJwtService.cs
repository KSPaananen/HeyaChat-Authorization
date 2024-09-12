namespace HeyaChat_Authorization.Services.Interfaces
{
    public interface IJwtService
    {
        string GenerateToken(int userID, string type);

        List<string> GetClaims(HttpRequest request);
    }
}

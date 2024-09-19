using HeyaChat_Authorization.DataObjects.DRO.SubClasses;

namespace HeyaChat_Authorization.Services.Interfaces
{
    public interface IJwtService
    {
        string GenerateToken(long userId, long deviceId, string type);

        long InvalidateToken();

        (Guid, long, string) GetClaims(HttpRequest request);

        bool VerifyToken(Guid jti, UserDevice device);

        string EncryptClaim(string value);

        string DecryptClaim(string value);
    }
}

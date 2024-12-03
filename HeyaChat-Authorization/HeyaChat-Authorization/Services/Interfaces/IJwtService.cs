using HeyaChat_Authorization.DataObjects.DRO.SubClasses;

namespace HeyaChat_Authorization.Services.Interfaces
{
    public interface IJwtService
    {
        string GenerateToken(long userId, long deviceId, string type);

        string RenewToken(long userId, long deviceId, string type, Guid oldJti);

        long InvalidateToken(Guid identifier);

        void InvalidateAllTokens(long userId);

        (bool isValid, bool expiresSoon) VerifyToken(Guid jti, UserDevice device);

        (Guid jti, long userId, string type) GetClaims(HttpRequest request);

        string EncryptClaim(string value);

        string DecryptClaim(string value);
    }
}

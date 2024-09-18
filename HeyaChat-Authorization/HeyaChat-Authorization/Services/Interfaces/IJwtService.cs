using HeyaChat_Authorization.DataObjects.DRO.SubClasses;

namespace HeyaChat_Authorization.Services.Interfaces
{
    public interface IJwtService
    {
        string GenerateToken(long userId, long deviceId, string type);

        long InvalidateToken();

        List<string> GetClaims(HttpRequest request);

        bool VerifyToken(Guid jti, UserDevice device);
    }
}

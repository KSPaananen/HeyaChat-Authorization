using HeyaChat_Authorization.DataObjects.DRO.SubClasses;

namespace HeyaChat_Authorization.DataObjects.DRO
{
    public class LoginDRO
    {
        public string Login { get; set; } = ""; // This can be username or password

        public string Password { get; set; } = "";

        public byte[] BiometricsKey { get; set; } = null!;

        public UserDevice Device { get; set; } = null!;
    }
}

using HeyaChat_Authorization.DataObjects.DRO.SubClasses;

namespace HeyaChat_Authorization.DataObjects.DRO
{
    public class LoginDRO
    {
        public string Login { get; set; } = null!; // This can be username or password

        public string Password { get; set; } = null!;

        public UserDevice Device { get; set; } = null!;
    }
}

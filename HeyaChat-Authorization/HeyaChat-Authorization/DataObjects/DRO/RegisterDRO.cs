using HeyaChat_Authorization.DataObjects.DRO.SubClasses;

namespace HeyaChat_Authorization.DataObjects.DRO
{
    public class RegisterDRO
    {
        public string Username { get; set; } = "";

        public string Password { get; set; } = "";

        public string Email { get; set; } = "";

        public UserDevice Device { get; set; } = null!;
    }
}

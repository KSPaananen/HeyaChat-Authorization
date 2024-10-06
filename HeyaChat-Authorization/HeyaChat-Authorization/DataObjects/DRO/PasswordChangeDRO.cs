using HeyaChat_Authorization.DataObjects.DRO.SubClasses;

namespace HeyaChat_Authorization.DataObjects.DRO
{
    public class PasswordChangeDRO
    {
        public string Password { get; set; } = "";

        public string PasswordRepeat { get; set; } = "";

        public UserDevice Device { get; set; } = null!;
    }
}

using HeyaChat_Authorization.DataObjects.DRO.SubClasses;

namespace HeyaChat_Authorization.DataObjects.DRO
{
    public class PasswordChangeDRO
    {
        public string Password { get; set; } = null!;

        public UserDevice Device { get; set; } = null!;
    }
}

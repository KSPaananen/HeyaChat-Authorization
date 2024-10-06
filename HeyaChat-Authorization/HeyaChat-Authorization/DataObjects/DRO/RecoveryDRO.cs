using HeyaChat_Authorization.DataObjects.DRO.SubClasses;

namespace HeyaChat_Authorization.DataObjects.DRO
{
    public class RecoveryDRO
    {
        public string Email { get; set; } = "";
        public UserDevice Device { get; set; } = null!;
    }
}

using HeyaChat_Authorization.DataObjects.DRO.SubClasses;

namespace HeyaChat_Authorization.DataObjects.DRO
{
    public class RecoveryDRO
    {
        public string email { get; set; } = null!;
        public UserDevice Device { get; set; } = null!;
    }
}
